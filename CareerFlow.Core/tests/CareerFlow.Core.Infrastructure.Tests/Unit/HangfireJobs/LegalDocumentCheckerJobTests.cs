using System.Net;
using System.Net.Http.Headers;
using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Infrastructure.HangfireJobs;
using Moq;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Unit.HangfireJobs;

public class LegalDocumentCheckerJobTests
{
    private readonly Mock<IAccountRepository> _accountRepoMock = new();
    private readonly Mock<IGithubPagesRequestsSender> _githubMock = new();
    private readonly LegalDocumentCheckerJob _sut;
    private readonly Mock<ISystemDocumentRepository> _systemDocumentRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    public LegalDocumentCheckerJobTests()
    {
        _sut = new LegalDocumentCheckerJob(
            _githubMock.Object,
            _accountRepoMock.Object,
            _uowMock.Object,
            _systemDocumentRepoMock.Object);
    }

    // ---------------------------------------------------------------------------
    // Constructor
    // ---------------------------------------------------------------------------

    [Fact]
    public void Constructor_NullGithubSender_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() =>
            new LegalDocumentCheckerJob(null!, _accountRepoMock.Object, _uowMock.Object,
                _systemDocumentRepoMock.Object));
    }

    [Fact]
    public void Constructor_NullAccountRepository_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() =>
            new LegalDocumentCheckerJob(_githubMock.Object, null!, _uowMock.Object, _systemDocumentRepoMock.Object));
    }

    [Fact]
    public void Constructor_NullUnitOfWork_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() =>
            new LegalDocumentCheckerJob(_githubMock.Object, _accountRepoMock.Object, null!,
                _systemDocumentRepoMock.Object));
    }

    [Fact]
    public void Constructor_NullSystemDocumentRepository_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() =>
            new LegalDocumentCheckerJob(_githubMock.Object, _accountRepoMock.Object, _uowMock.Object, null!));
    }

    // ---------------------------------------------------------------------------
    // ETag guards
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task CheckForUpdatesAsync_NullEtag_DoesNotSaveChanges()
    {
        // Arrange
        _githubMock.Setup(g => g.GetContentAsync("Terms", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildHttpResponse(HttpStatusCode.OK, null));

        // Act
        await _sut.CheckForUpdatesAsync("Terms");

        // Assert
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckForUpdatesAsync_HttpError_ThrowsHttpRequestException()
    {
        // Arrange
        _githubMock.Setup(g => g.GetContentAsync("Terms", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildHttpResponse(HttpStatusCode.InternalServerError, null));

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(() => _sut.CheckForUpdatesAsync("Terms"));
    }

    // ---------------------------------------------------------------------------
    // New document
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task CheckForUpdatesAsync_NewDocument_AddsDocumentAndSavesOnce()
    {
        // Arrange
        _githubMock.Setup(g => g.GetContentAsync("Terms", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildHttpResponse(HttpStatusCode.OK, "\"etag123\""));

        _systemDocumentRepoMock
            .Setup(r => r.FindByTypeAsync("Terms", It.IsAny<CancellationToken>()))
            .ReturnsAsync((SystemDocument?)null);

        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _sut.CheckForUpdatesAsync("Terms");

        // Assert
        _systemDocumentRepoMock.Verify(
            r => r.AddAsync(It.Is<SystemDocument>(d => d.DocumentType == "Terms" && d.CurrentETag == "\"etag123\""),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckForUpdatesAsync_NewDocument_DoesNotNotifyAccounts()
    {
        // Arrange
        _githubMock.Setup(g => g.GetContentAsync("Terms", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildHttpResponse(HttpStatusCode.OK, "\"etag123\""));

        _systemDocumentRepoMock
            .Setup(r => r.FindByTypeAsync("Terms", It.IsAny<CancellationToken>()))
            .ReturnsAsync((SystemDocument?)null);

        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _sut.CheckForUpdatesAsync("Terms");

        // Assert
        _accountRepoMock.Verify(
            a => a.UpdateTermsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ---------------------------------------------------------------------------
    // Same ETag
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task CheckForUpdatesAsync_SameEtag_DoesNotUpdateOrNotifyAccounts()
    {
        // Arrange
        var existing = SystemDocument.Create("Terms", "\"same\"");

        _githubMock.Setup(g => g.GetContentAsync("Terms", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildHttpResponse(HttpStatusCode.OK, "\"same\""));

        _systemDocumentRepoMock
            .Setup(r => r.FindByTypeAsync("Terms", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        await _sut.CheckForUpdatesAsync("Terms");

        // Assert
        _accountRepoMock.Verify(a => a.UpdateTermsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------------------------------------------------------------------------
    // Different ETag
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task CheckForUpdatesAsync_DifferentEtag_UpdatesDocumentAndNotifiesAccounts()
    {
        // Arrange
        var existing = SystemDocument.Create("Terms", "\"old\"");

        _githubMock.Setup(g => g.GetContentAsync("Terms", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildHttpResponse(HttpStatusCode.OK, "\"new\""));

        _systemDocumentRepoMock
            .Setup(r => r.FindByTypeAsync("Terms", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _accountRepoMock.Setup(a => a.UpdateTermsAsync("Terms", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.CheckForUpdatesAsync("Terms");

        // Assert
        _accountRepoMock.Verify(a => a.UpdateTermsAsync("Terms", It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckForUpdatesAsync_DifferentEtag_CommitsTransaction()
    {
        // Arrange
        var existing = SystemDocument.Create("Terms", "\"old\"");

        _githubMock.Setup(g => g.GetContentAsync("Terms", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildHttpResponse(HttpStatusCode.OK, "\"new\""));

        _systemDocumentRepoMock
            .Setup(r => r.FindByTypeAsync("Terms", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _sut.CheckForUpdatesAsync("Terms");

        // Assert
        _uowMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckForUpdatesAsync_DifferentEtag_WhenSaveFails_RollsBack()
    {
        // Arrange
        var existing = SystemDocument.Create("Terms", "\"old\"");

        _githubMock.Setup(g => g.GetContentAsync("Terms", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildHttpResponse(HttpStatusCode.OK, "\"new\""));

        _systemDocumentRepoMock
            .Setup(r => r.FindByTypeAsync("Terms", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db error"));

        // Act
        await Should.ThrowAsync<InvalidOperationException>(() => _sut.CheckForUpdatesAsync("Terms"));

        // Assert
        _uowMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------------------------------------------------------------------------
    // Helper
    // ---------------------------------------------------------------------------

    private static HttpResponseMessage BuildHttpResponse(HttpStatusCode status, string? etag)
    {
        var response = new HttpResponseMessage(status);
        if (etag is not null)
            response.Headers.ETag = new EntityTagHeaderValue(etag);
        return response;
    }
}
