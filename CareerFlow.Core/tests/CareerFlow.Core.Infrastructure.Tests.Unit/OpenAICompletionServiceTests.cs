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

public sealed class OpenAICompletionServiceTests
{
    private readonly Mock<IOpenAIHttpClient> _httpClientMock = new();
    private readonly OpenAIOptions _options = new() { DefaultModel = "gpt-4o-mini" };
    private readonly OpenAICompletionService _sut;

    public OpenAICompletionServiceTests()
    {
        _sut = new OpenAICompletionService(_httpClientMock.Object, Options.Create(_options));
    }

    private static ChatResponse BuildChatResponse(string content, int tokens = 10, string finish = "stop")
        => new(
            Choices: [new ChatChoiceDto(new ChatMessageDto("assistant", content), finish)],
            Usage: new ChatUsageDto(tokens,tokens,tokens));

    [Fact]
    public async Task CompleteAsync_WithPromptOnly_SendsToChatCompletionsEndpoint()
    {https://platform.openai.com/login
        // Arrange
        _httpClientMock
            .Setup(c => c.PostAsync<ChatRequest, ChatResponse>(
                "chat/completions", It.IsAny<ChatRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildChatResponse("Hi there"));

        // Act
        await _sut.CompleteAsync(new CompletionRequest(Prompt: "Hello"));

        // Assert
        _httpClientMock.Verify(c => c.PostAsync<ChatRequest, ChatResponse>(
            "chat/completions", It.IsAny<ChatRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_PromptOnly_WrapsInUserMessage()
    {
        // Arrange
        const string prompt = "What is .NET?";
        ChatRequest? capturedPayload = null;

        _httpClientMock
            .Setup(c => c.PostAsync<ChatRequest, ChatResponse>(
                It.IsAny<string>(), It.IsAny<ChatRequest>(), It.IsAny<CancellationToken>()))
            .Callback<string, ChatRequest, CancellationToken>((_, payload, _) => capturedPayload = payload)
            .ReturnsAsync(BuildChatResponse("It is a framework"));

        // Act
        await _sut.CompleteAsync(new CompletionRequest(Prompt: prompt));

        // Assert
        capturedPayload.ShouldNotBeNull();
        capturedPayload.Messages.Count.ShouldBe(1);
        capturedPayload.Messages[0].Role.ShouldBe("user");
        capturedPayload.Messages[0].Content.ShouldBe(prompt);
    }

    [Fact]
    public async Task CompleteAsync_UsesDefaultModelWhenRequestModelIsNull()
    {
        // Arrange
        ChatRequest? captured = null;

        _httpClientMock
            .Setup(c => c.PostAsync<ChatRequest, ChatResponse>(
                It.IsAny<string>(), It.IsAny<ChatRequest>(), It.IsAny<CancellationToken>()))
            .Callback<string, ChatRequest, CancellationToken>((_, req, _) => captured = req)
            .ReturnsAsync(BuildChatResponse("ok"));

        // Act
        await _sut.CompleteAsync(new CompletionRequest(Prompt: "test", Model: null));

        // Assert
        captured.ShouldNotBeNull();
        captured.Model.ShouldBe(_options.DefaultModel);
    }

    [Fact]
    public async Task CompleteAsync_UsesRequestModelWhenProvided()
    {
        // Arrange
        const string customModel = "gpt-4o";
        ChatRequest? captured = null;

        _httpClientMock
            .Setup(c => c.PostAsync<ChatRequest, ChatResponse>(
                It.IsAny<string>(), It.IsAny<ChatRequest>(), It.IsAny<CancellationToken>()))
            .Callback<string, ChatRequest, CancellationToken>((_, req, _) => captured = req)
            .ReturnsAsync(BuildChatResponse("ok"));

        // Act
        await _sut.CompleteAsync(new CompletionRequest(Prompt: "test", Model: customModel));

        // Assert
        captured.ShouldNotBeNull();
        captured.Model.ShouldBe(customModel);
    }

    [Fact]
    public async Task CompleteAsync_WithMessages_ForwardsAllMessages()
    {
        // Arrange
        var messages = new List<Message>
        {
            new("system", "You are an assistant"),
            new("user", "Hello")
        };
        ChatRequest? captured = null;

        _httpClientMock
            .Setup(c => c.PostAsync<ChatRequest, ChatResponse>(
                It.IsAny<string>(), It.IsAny<ChatRequest>(), It.IsAny<CancellationToken>()))
            .Callback<string, ChatRequest, CancellationToken>((_, req, _) => captured = req)
            .ReturnsAsync(BuildChatResponse("ok"));

        // Act
        await _sut.CompleteAsync(new CompletionRequest(Prompt: "unused", Messages: messages));

        // Assert
        captured.ShouldNotBeNull();
        captured.Messages.Count.ShouldBe(2);
        captured.Messages[0].Role.ShouldBe("system");
        captured.Messages[1].Role.ShouldBe("user");
    }

    [Fact]
    public async Task CompleteAsync_MapsResponseCorrectly()
    {
        // Arrange
        const string expectedContent = "Mapped content";
        const int expectedTokens = 99;
        const string expectedFinish = "length";

        _httpClientMock
            .Setup(c => c.PostAsync<ChatRequest, ChatResponse>(
                It.IsAny<string>(), It.IsAny<ChatRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildChatResponse(expectedContent, expectedTokens, expectedFinish));

        // Act
        var result = await _sut.CompleteAsync(new CompletionRequest(Prompt: "map me"));

        // Assert
        result.Content.ShouldBe(expectedContent);
        result.TokensUsed.ShouldBe(expectedTokens);
        result.FinishReason.ShouldBe(expectedFinish);
    }

    [Fact]
    public async Task CompleteAsync_ForwardsMaxTokensAndTemperature()
    {
        // Arrange
        ChatRequest? captured = null;

        _httpClientMock
            .Setup(c => c.PostAsync<ChatRequest, ChatResponse>(
                It.IsAny<string>(), It.IsAny<ChatRequest>(), It.IsAny<CancellationToken>()))
            .Callback<string, ChatRequest, CancellationToken>((_, req, _) => captured = req)
            .ReturnsAsync(BuildChatResponse("ok"));

        // Act
        await _sut.CompleteAsync(new CompletionRequest(Prompt: "x", MaxTokens: 256, Temperature: 0.2f));

        // Assert
        captured.ShouldNotBeNull();
        captured.MaxTokens.ShouldBe(256);
        captured.Temperature.ShouldBe(0.2f, tolerance: 0.001f);
    }

    [Fact]
    public async Task CompleteAsync_HttpClientThrows_PropagatesException()
    {
        // Arrange
        _httpClientMock
            .Setup(c => c.PostAsync<ChatRequest, ChatResponse>(
                It.IsAny<string>(), It.IsAny<ChatRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OpenAIException(401,"Unauthorized"));

        // Act & Assert
        var ex = await Should.ThrowAsync<OpenAIException>(
            () => _sut.CompleteAsync(new CompletionRequest(Prompt: "boom")));

        ex.StatusCode.ShouldBe(401);
    }

    [Fact]
    public async Task CompleteAsync_PassesCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _httpClientMock
            .Setup(c => c.PostAsync<ChatRequest, ChatResponse>(
                It.IsAny<string>(), It.IsAny<ChatRequest>(), token))
            .ReturnsAsync(BuildChatResponse("ok"));

        // Act
        await _sut.CompleteAsync(new CompletionRequest(Prompt: "x"), token);

        // Assert
        _httpClientMock.Verify(c => c.PostAsync<ChatRequest, ChatResponse>(
            It.IsAny<string>(), It.IsAny<ChatRequest>(), token), Times.Once);
    }
}