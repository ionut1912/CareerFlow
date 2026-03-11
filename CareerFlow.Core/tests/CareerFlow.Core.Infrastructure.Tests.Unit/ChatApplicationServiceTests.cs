using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.Models.OpenAi;
using CareerFlow.Core.Infrastructure.Services.OpenAi;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Unit;

public sealed class ChatApplicationServiceTests
{
    private readonly Mock<IAICompletionService> _completionServiceMock = new();
    private readonly Mock<ILogger<ChatApplicationService>> _loggerMock = new();
    private readonly ChatApplicationService _sut;

    public ChatApplicationServiceTests()
    {
        _sut = new ChatApplicationService(_completionServiceMock.Object, _loggerMock.Object);
    }

    // ── AskAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task AskAsync_ValidMessage_ReturnsCompletionContent()
    {
        // Arrange
        const string userMessage = "Hello, how are you?";
        const string expectedContent = "I am fine, thank you!";

        _completionServiceMock
            .Setup(s => s.CompleteAsync(It.IsAny<CompletionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CompletionResult(Content: expectedContent, TokensUsed: 10, FinishReason: "stop"));

        // Act
        var result = await _sut.AskAsync(userMessage);

        // Assert
        result.ShouldBe(expectedContent);
    }

    [Fact]
    public async Task AskAsync_BuildsRequestWithCorrectDefaults()
    {
        // Arrange
        const string userMessage = "Tell me something";
        CompletionRequest? capturedRequest = null;

        _completionServiceMock
            .Setup(s => s.CompleteAsync(It.IsAny<CompletionRequest>(), It.IsAny<CancellationToken>()))
            .Callback<CompletionRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new CompletionResult("response", 5, "stop"));

        // Act
        await _sut.AskAsync(userMessage);

        // Assert
        capturedRequest.ShouldNotBeNull();
        capturedRequest.Prompt.ShouldBe(userMessage);
        capturedRequest.MaxTokens.ShouldBe(500);
        capturedRequest.Temperature.ShouldBe(0.7f, tolerance: 0.001f);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AskAsync_NullOrWhiteSpaceMessage_ThrowsArgumentException(string? invalidMessage)
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(() => _sut.AskAsync(invalidMessage!));
    }

    [Fact]
    public async Task AskAsync_PassesCancellationToken_ToCompletionService()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _completionServiceMock
            .Setup(s => s.CompleteAsync(It.IsAny<CompletionRequest>(), token))
            .ReturnsAsync(new CompletionResult("ok", 1, "stop"));

        // Act
        await _sut.AskAsync("message", token);

        // Assert
        _completionServiceMock.Verify(s =>
            s.CompleteAsync(It.IsAny<CompletionRequest>(), token), Times.Once);
    }

    [Fact]
    public async Task AskAsync_CompletionServiceThrows_PropagatesException()
    {
        // Arrange
        _completionServiceMock
            .Setup(s => s.CompleteAsync(It.IsAny<CompletionRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OpenAIException(500,"Service error"));

        // Act & Assert
        var ex = await Should.ThrowAsync<OpenAIException>(() => _sut.AskAsync("Hello"));
        ex.Message.ShouldBe("Service error");
    }

    // ── AskWithSystemPromptAsync ───────────────────────────────────────────────

    [Fact]
    public async Task AskWithSystemPromptAsync_ValidInputs_ReturnsContent()
    {
        // Arrange
        const string systemPrompt = "You are a career coach.";
        const string userMessage = "Help me with my CV.";
        const string expectedContent = "Sure, here is how...";

        _completionServiceMock
            .Setup(s => s.CompleteAsync(It.IsAny<CompletionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CompletionResult(expectedContent, 20, "stop"));

        // Act
        var result = await _sut.AskWithSystemPromptAsync(systemPrompt, userMessage);

        // Assert
        result.ShouldBe(expectedContent);
    }

    [Fact]
    public async Task AskWithSystemPromptAsync_BuildsRequestWithBothMessages()
    {
        // Arrange
        const string systemPrompt = "Be concise.";
        const string userMessage = "What is AI?";
        CompletionRequest? capturedRequest = null;

        _completionServiceMock
            .Setup(s => s.CompleteAsync(It.IsAny<CompletionRequest>(), It.IsAny<CancellationToken>()))
            .Callback<CompletionRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new CompletionResult("AI is...", 8, "stop"));

        // Act
        await _sut.AskWithSystemPromptAsync(systemPrompt, userMessage);

        // Assert
        capturedRequest.ShouldNotBeNull();
        capturedRequest.Messages.ShouldNotBeNull();
        capturedRequest.Messages!.Count.ShouldBe(2);
        capturedRequest.Messages![0].Role.ShouldBe("system");
        capturedRequest.Messages![0].Content.ShouldBe(systemPrompt);
        capturedRequest.Messages![1].Role.ShouldBe("user");
        capturedRequest.Messages![1].Content.ShouldBe(userMessage);
    }

    [Theory]
    [InlineData(null, "valid")]
    [InlineData("", "valid")]
    [InlineData("   ", "valid")]
    [InlineData("valid", null)]
    [InlineData("valid", "")]
    [InlineData("valid", "   ")]
    public async Task AskWithSystemPromptAsync_NullOrWhiteSpaceArgs_ThrowsArgumentException(
        string? systemPrompt, string? userMessage)
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(
            () => _sut.AskWithSystemPromptAsync(systemPrompt!, userMessage!));
    }

    [Fact]
    public async Task AskWithSystemPromptAsync_PassesCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _completionServiceMock
            .Setup(s => s.CompleteAsync(It.IsAny<CompletionRequest>(), token))
            .ReturnsAsync(new CompletionResult("ok", 1, "stop"));

        // Act
        await _sut.AskWithSystemPromptAsync("system", "user", token);

        // Assert
        _completionServiceMock.Verify(s =>
            s.CompleteAsync(It.IsAny<CompletionRequest>(), token), Times.Once);
    }
}
