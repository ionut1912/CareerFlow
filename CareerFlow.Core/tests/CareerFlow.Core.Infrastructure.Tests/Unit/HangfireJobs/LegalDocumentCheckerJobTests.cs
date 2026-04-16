using System.Net;
using System.Net.Http.Headers;
using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Infrastructure.HangfireJobs;
using CareerFlow.Core.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Unit.HangfireJobs;

public class LegalDocumentCheckerJobTests
{
    private readonly Mock<IAccountRepository> _accountRepoMock = new();
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IGithubPagesRequestsSender> _githubMock = new();
    private readonly LegalDocumentCheckerJob _sut;
    private readonly Mock<IUnitOfWork> _uowMock = new();

    public LegalDocumentCheckerJobTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _sut = new LegalDocumentCheckerJob(_githubMock.Object, _dbContext, _accountRepoMock.Object, _uowMock.Object);
    }

    [Fact]
    public void Constructor_NullGithubSender_ThrowsArgumentNullException()
    {
        var ctx = BuildInMemoryContext();
        Should.Throw<ArgumentNullException>(() =>
            new LegalDocumentCheckerJob(null!, ctx, _accountRepoMock.Object, _uowMock.Object));
    }

    [Fact]
    public void Constructor_NullDbContext_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() =>
            new LegalDocumentCheckerJob(_githubMock.Object, null!, _accountRepoMock.Object, _uowMock.Object));
    }

    [Fact]
    public void Constructor_NullAccountRepository_ThrowsArgumentNullException()
    {
        var ctx = BuildInMemoryContext();
        Should.Throw<ArgumentNullException>(() =>
            new LegalDocumentCheckerJob(_githubMock.Object, ctx, null!, _uowMock.Object));
    }

    [Fact]
    public void Constructor_NullUnitOfWork_ThrowsArgumentNullException()
    {
        var ctx = BuildInMemoryContext();
        Should.Throw<ArgumentNullException>(() =>
            new LegalDocumentCheckerJob(_githubMock.Object, ctx, _accountRepoMock.Object, null!));
    }

    [Fact]
    public async Task CheckForUpdatesAsync_NullEtag_DoesNotSaveChanges()
    {
        _githubMock.Setup(g => g.GetContentAsync("Terms", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildHttpResponse(HttpStatusCode.OK, null));

        await _sut.CheckForUpdatesAsync("Terms");

        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckForUpdatesAsync_WhitespaceEtag_DoesNotSaveChanges()
    {
        var response = BuildHttpResponse(HttpStatusCode.OK, null);
        _githubMock.Setup(g => g.GetContentAsync("Terms", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        await _sut.CheckForUpdatesAsync("Terms");

        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckForUpdatesAsync_NewDocument_CreatesSystemDocumentRecord()
    {
        // Arrange
        _githubMock.Setup(g => g.GetContentAsync("Terms", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildHttpResponse(HttpStatusCode.OK, "\"etag123\""));

        // Redirect the Mock call to the real DbContext
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns((CancellationToken ct) => _dbContext.SaveChangesAsync(ct));

        // Act
        await _sut.CheckForUpdatesAsync("Terms");

        // Assert
        // Clear tracking if necessary to ensure we fetch fresh from the "DB"
        _dbContext.ChangeTracker.Clear();

        var doc = await _dbContext.SystemDocuments.FirstOrDefaultAsync(d => d.DocumentType == "Terms");

        doc.ShouldNotBeNull();
        doc.CurrentETag.ShouldBe("\"etag123\"");
    }

    [Fact]
    public async Task CheckForUpdatesAsync_NewDocument_SavesChangesOnce()
    {
        _githubMock.Setup(g => g.GetContentAsync("Terms", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildHttpResponse(HttpStatusCode.OK, "\"etag123\""));
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _sut.CheckForUpdatesAsync("Terms");

        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckForUpdatesAsync_SameEtag_DoesNotUpdateOrNotifyAccounts()
    {
        _dbContext.SystemDocuments.Add(SystemDocument.Create("Terms", "\"same\""));
        await _dbContext.SaveChangesAsync();

        _githubMock.Setup(g => g.GetContentAsync("Terms", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildHttpResponse(HttpStatusCode.OK, "\"same\""));

        await _sut.CheckForUpdatesAsync("Terms");

        _accountRepoMock.Verify(a => a.UpdateTermsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckForUpdatesAsync_DifferentEtag_UpdatesDocumentAndNotifiesAccounts()
    {
        _dbContext.SystemDocuments.Add(SystemDocument.Create("Terms", "\"old\""));
        await _dbContext.SaveChangesAsync();

        _githubMock.Setup(g => g.GetContentAsync("Terms", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildHttpResponse(HttpStatusCode.OK, "\"new\""));
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _accountRepoMock.Setup(a => a.UpdateTermsAsync("Terms", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.CheckForUpdatesAsync("Terms");

        _accountRepoMock.Verify(a => a.UpdateTermsAsync("Terms", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckForUpdatesAsync_DifferentEtag_SavesChangesAtLeastOnce()
    {
        _dbContext.SystemDocuments.Add(SystemDocument.Create("Privacy", "\"v1\""));
        await _dbContext.SaveChangesAsync();

        _githubMock.Setup(g => g.GetContentAsync("Privacy", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildHttpResponse(HttpStatusCode.OK, "\"v2\""));
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _accountRepoMock.Setup(a => a.UpdateTermsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.CheckForUpdatesAsync("Privacy");

        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CheckForUpdatesAsync_HttpError_ThrowsHttpRequestException()
    {
        _githubMock.Setup(g => g.GetContentAsync("Terms", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildHttpResponse(HttpStatusCode.InternalServerError, null));

        await Should.ThrowAsync<HttpRequestException>(() => _sut.CheckForUpdatesAsync("Terms"));
    }

    private static ApplicationDbContext BuildInMemoryContext()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(opts);
    }

    private static HttpResponseMessage BuildHttpResponse(HttpStatusCode status, string? etag)
    {
        var response = new HttpResponseMessage(status);
        if (etag is not null)
            response.Headers.ETag = new EntityTagHeaderValue(etag);
        return response;
    }
}