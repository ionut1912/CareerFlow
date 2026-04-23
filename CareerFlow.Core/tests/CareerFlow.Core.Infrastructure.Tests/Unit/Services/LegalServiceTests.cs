using System.Net;

using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Models.Legal;
using CareerFlow.Core.Infrastructure.Services;

using Microsoft.Extensions.Logging;

using Moq;

using Shouldly;

using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Unit.Services;

public class LegalServiceTests
{
    private readonly Mock<ICacheService> _cacheMock;
    private readonly Mock<ILogger<LegalService>> _loggerMock;
    private readonly Mock<IGithubPagesRequestsSender> _requestsSenderMock;
    private readonly LegalService _sut;

    public LegalServiceTests()
    {
        _requestsSenderMock = new Mock<IGithubPagesRequestsSender>();
        _cacheMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<LegalService>>();

        _sut = new LegalService(
            _requestsSenderMock.Object,
            _cacheMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetDocumentAsync_WhenCacheHit_ShouldReturnCachedDocumentWithoutCallingGithub()
    {
        // Arrange
        const string docType = "terms-of-service";
        var cachedDocument = new LegalDocumentResponse("Cached Content", "GitHub Pages", DateTime.UtcNow);

        _cacheMock
            .Setup(x => x.GetAsync<LegalDocumentResponse>($"legal:{docType}"))
            .ReturnsAsync(cachedDocument);

        // Act
        LegalDocumentResponse? result = await _sut.GetDocumentAsync(docType, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(cachedDocument);
        _requestsSenderMock.Verify(x => x.GetContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetDocumentAsync_WhenCacheMissAndResponseIsSuccess_ShouldReturnDocumentAndCacheIt()
    {
        // Arrange
        const string docType = "terms-of-service";
        const string content = "Standard Legal Text";
        const string cacheKey = $"legal:{docType}";

        _cacheMock
            .Setup(x => x.GetAsync<LegalDocumentResponse>(cacheKey))
            .ReturnsAsync((LegalDocumentResponse?)null);

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(content) };

        _requestsSenderMock
            .Setup(x => x.GetContentAsync(docType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        LegalDocumentResponse? result = await _sut.GetDocumentAsync(docType, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Content.ShouldBe(content);
        result.Source.ShouldBe("GitHub Pages");

        _cacheMock.Verify(x =>
                x.SetAsync(cacheKey, result, TimeSpan.FromHours(6)),
            Times.Once);
    }

    [Fact]
    public async Task GetDocumentAsync_WhenCacheMissAndResponseFails_ShouldReturnNullWithoutCaching()
    {
        // Arrange
        _cacheMock
            .Setup(x => x.GetAsync<LegalDocumentResponse>(It.IsAny<string>()))
            .ReturnsAsync((LegalDocumentResponse?)null);

        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        _requestsSenderMock
            .Setup(x => x.GetContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        // Act
        LegalDocumentResponse? result = await _sut.GetDocumentAsync("any-type", CancellationToken.None);

        // Assert
        result.ShouldBeNull();
        _cacheMock.Verify(x =>
                x.SetAsync(It.IsAny<string>(), It.IsAny<LegalDocumentResponse>(), It.IsAny<TimeSpan>()),
            Times.Never);
    }

    [Fact]
    public void Constructor_WhenSenderIsNull_ShouldThrowArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() =>
            new LegalService(null!, _cacheMock.Object, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_WhenCacheIsNull_ShouldThrowArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() =>
            new LegalService(_requestsSenderMock.Object, null!, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() =>
            new LegalService(_requestsSenderMock.Object, _cacheMock.Object, null!));
    }
}
