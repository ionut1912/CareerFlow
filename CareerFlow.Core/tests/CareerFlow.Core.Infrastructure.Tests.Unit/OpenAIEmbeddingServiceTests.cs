using CareerFlow.Core.Domain.Abstractions.Http;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.Models.OpenAi;
using CareerFlow.Core.Infrastructure.Configurations;
using CareerFlow.Core.Infrastructure.Services.OpenAi;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Unit;

public sealed class OpenAIEmbeddingServiceTests
{
    private readonly Mock<IOpenAIHttpClient> _httpClientMock = new();
    private readonly OpenAIEmbeddingService _sut;

    public OpenAIEmbeddingServiceTests()
    {
        var options = Options.Create(new OpenAIOptions { DefaultModel = "gpt-4o-mini" });
        _sut = new OpenAIEmbeddingService(_httpClientMock.Object, options);
    }

    private static EmbeddingResponse BuildEmbeddingResponse(float[] vector, int tokens = 5)
        => new(
            Data: [new EmbeddingDataDto(Embedding: vector, Index: 0)],
            Usage: new EmbeddingUsageDto(PromptTokens: tokens, TotalTokens: tokens));
    [Fact]
    public async Task GetEmbeddingAsync_ValidText_CallsEmbeddingsEndpoint()
    {
        // Arrange
        _httpClientMock
            .Setup(c => c.PostAsync<EmbeddingRequest, EmbeddingResponse>(
                "embeddings", It.IsAny<EmbeddingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildEmbeddingResponse([0.1f, 0.2f, 0.3f]));

        // Act
        await _sut.GetEmbeddingAsync("some text");

        // Assert
        _httpClientMock.Verify(c => c.PostAsync<EmbeddingRequest, EmbeddingResponse>(
            "embeddings", It.IsAny<EmbeddingRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetEmbeddingAsync_AlwaysUsesTextEmbeddingModel()
    {
        // Arrange
        EmbeddingRequest? captured = null;

        _httpClientMock
            .Setup(c => c.PostAsync<EmbeddingRequest, EmbeddingResponse>(
                It.IsAny<string>(), It.IsAny<EmbeddingRequest>(), It.IsAny<CancellationToken>()))
            .Callback<string, EmbeddingRequest, CancellationToken>((_, req, _) => captured = req)
            .ReturnsAsync(BuildEmbeddingResponse([0.1f]));

        // Act
        await _sut.GetEmbeddingAsync("embed this");

        // Assert
        captured.ShouldNotBeNull();
        captured.Model.ShouldBe("text-embedding-3-small");
    }

    [Fact]
    public async Task GetEmbeddingAsync_ForwardsInputText()
    {
        // Arrange
        const string inputText = "My career summary";
        EmbeddingRequest? captured = null;

        _httpClientMock
            .Setup(c => c.PostAsync<EmbeddingRequest, EmbeddingResponse>(
                It.IsAny<string>(), It.IsAny<EmbeddingRequest>(), It.IsAny<CancellationToken>()))
            .Callback<string, EmbeddingRequest, CancellationToken>((_, req, _) => captured = req)
            .ReturnsAsync(BuildEmbeddingResponse([0.5f]));

        // Act
        await _sut.GetEmbeddingAsync(inputText);

        // Assert
        captured.ShouldNotBeNull();
        captured.Input.ShouldBe(inputText);
    }

    [Fact]
    public async Task GetEmbeddingAsync_MapsVectorCorrectly()
    {
        // Arrange
        float[] expectedVector = [0.1f, 0.5f, 0.9f, -0.3f];

        _httpClientMock
            .Setup(c => c.PostAsync<EmbeddingRequest, EmbeddingResponse>(
                It.IsAny<string>(), It.IsAny<EmbeddingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildEmbeddingResponse(expectedVector));

        // Act
        var result = await _sut.GetEmbeddingAsync("test");

        // Assert
        result.Vector.ShouldBe(expectedVector);
    }

    [Fact]
    public async Task GetEmbeddingAsync_MapsTokensUsedCorrectly()
    {
        // Arrange
        const int expectedTokens = 33;

        _httpClientMock
            .Setup(c => c.PostAsync<EmbeddingRequest, EmbeddingResponse>(
                It.IsAny<string>(), It.IsAny<EmbeddingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildEmbeddingResponse([0.1f], expectedTokens));

        // Act
        var result = await _sut.GetEmbeddingAsync("tokens");

        // Assert
        result.TokensUsed.ShouldBe(expectedTokens);
    }

    [Fact]
    public async Task GetEmbeddingAsync_HttpClientThrows_PropagatesException()
    {
        // Arrange
        _httpClientMock
            .Setup(c => c.PostAsync<EmbeddingRequest, EmbeddingResponse>(
                It.IsAny<string>(), It.IsAny<EmbeddingRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OpenAIException(500,"Internal Server Error"));

        // Act & Assert
        var ex = await Should.ThrowAsync<OpenAIException>(
            () => _sut.GetEmbeddingAsync("bad input"));

        ex.StatusCode.ShouldBe(500);
    }

    [Fact]
    public async Task GetEmbeddingAsync_PassesCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _httpClientMock
            .Setup(c => c.PostAsync<EmbeddingRequest, EmbeddingResponse>(
                It.IsAny<string>(), It.IsAny<EmbeddingRequest>(), token))
            .ReturnsAsync(BuildEmbeddingResponse([0.1f]));

        // Act
        await _sut.GetEmbeddingAsync("cancel test", token);

        // Assert
        _httpClientMock.Verify(c => c.PostAsync<EmbeddingRequest, EmbeddingResponse>(
            It.IsAny<string>(), It.IsAny<EmbeddingRequest>(), token), Times.Once);
    }

    [Fact]
    public async Task GetEmbeddingAsync_LargeEmbeddingVector_HandledCorrectly()
    {
        // Arrange
        var largeVector = new float[1536]; // standard OpenAI embedding dimension
        for (var i = 0; i < largeVector.Length; i++)
            largeVector[i] = (float)Math.Sin(i);

        _httpClientMock
            .Setup(c => c.PostAsync<EmbeddingRequest, EmbeddingResponse>(
                It.IsAny<string>(), It.IsAny<EmbeddingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildEmbeddingResponse(largeVector, 50));

        // Act
        var result = await _sut.GetEmbeddingAsync("large text");

        // Assert
        result.Vector.Length.ShouldBe(1536);
        result.Vector[0].ShouldBe((float)Math.Sin(0), tolerance: 0.001f);
    }
}
