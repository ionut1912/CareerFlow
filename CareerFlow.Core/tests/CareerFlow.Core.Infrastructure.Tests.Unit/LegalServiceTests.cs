using System.Net;
using CareerFlow.Core.Infrastructure.Configurations;
using CareerFlow.Core.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Unit;

public class LegalServiceTests : IDisposable
{
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<IOptions<LegalDocSettings>> _optionsMock;
    private readonly LegalDocSettings _settings;

    public LegalServiceTests()
    {
        _settings = new LegalDocSettings { GitHubPagesBaseUrl = "https://legal.careerflow.com/" };
        _optionsMock = new Mock<IOptions<LegalDocSettings>>();
        _optionsMock.Setup(x => x.Value).Returns(_settings);

        _handlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_handlerMock.Object);
    }

    [Theory]
    [InlineData("privacy", "privacy.md")]
    [InlineData("terms", "terms.md")]
    [InlineData("PRIVACY", "privacy.md")]
    public async Task GetDocumentAsync_ShouldConstructCorrectUrl_AndReturnSuccess(string type, string expectedFileName)
    {
        var expectedContent = "# Legal Content";
        SetupHttpMessage(HttpStatusCode.OK, expectedContent);

        var service = new LegalService(_httpClient, _optionsMock.Object);

        var result = await service.GetDocumentAsync(type, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Content.ShouldBe(expectedContent);
        result.Source.ShouldBe("GitHub Pages");

        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri!.ToString().EndsWith(expectedFileName)),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetDocumentAsync_ShouldReturnNull_WhenGithubReturns404()
    {
        SetupHttpMessage(HttpStatusCode.NotFound, "");

        var service = new LegalService(_httpClient, _optionsMock.Object);

        var result = await service.GetDocumentAsync("privacy", CancellationToken.None);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetDocumentAsync_ShouldHandleBaseUrlWithoutTrailingSlash()
    {
        _settings.GitHubPagesBaseUrl = "https://legal.careerflow.com";
        SetupHttpMessage(HttpStatusCode.OK, "content");

        var service = new LegalService(_httpClient, _optionsMock.Object);

        var result = await service.GetDocumentAsync("terms", CancellationToken.None);

        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetDocumentAsync_ShouldThrowException_WhenHttpClientFails()
    {
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var service = new LegalService(_httpClient, _optionsMock.Object);

        await Should.ThrowAsync<HttpRequestException>(async () =>
            await service.GetDocumentAsync("privacy", CancellationToken.None));
    }

    [Fact]
    public async Task GetDocumentAsync_ShouldRespectCancellationToken()
    {
        var cts = new CancellationTokenSource();

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Returns<HttpRequestMessage, CancellationToken>(async (request, token) =>
            {
                await cts.CancelAsync();
                throw new OperationCanceledException(cts.Token);
            });

        var service = new LegalService(_httpClient, _optionsMock.Object);

        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await service.GetDocumentAsync("privacy", cts.Token));
    }

    private void SetupHttpMessage(HttpStatusCode statusCode, string content)
    {
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}