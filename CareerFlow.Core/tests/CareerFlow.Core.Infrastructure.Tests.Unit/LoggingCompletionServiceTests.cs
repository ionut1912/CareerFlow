using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.Models.OpenAi;
using CareerFlow.Core.Infrastructure.Services.OpenAi;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Unit;

public sealed class LoggingCompletionServiceTests
{
    private readonly Mock<IAICompletionService> _innerMock = new();
    private readonly Mock<ILogger<LoggingCompletionService>> _loggerMock = new();
    private readonly LoggingCompletionService _sut;

    public LoggingCompletionServiceTests()
    {
        _sut = new LoggingCompletionService(_innerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CompleteAsync_Success_ReturnsInnerResult()
    {
        // Arrange
        var request = new CompletionRequest(Prompt: "test");
        var expected = new CompletionResult("response", 42, "stop");

        _innerMock
            .Setup(s => s.CompleteAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.CompleteAsync(request);

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public async Task CompleteAsync_Success_LogsInformationTwice()
    {
        // Arrange
        var request = new CompletionRequest(Prompt: "log me");

        _innerMock
            .Setup(s => s.CompleteAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CompletionResult("ok", 5, "stop"));

        // Act
        await _sut.CompleteAsync(request);

        // Assert – at least 2 information-level log calls (start + success)
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeast(2));
    }

    [Fact]
    public async Task CompleteAsync_OpenAIException_LogsErrorAndRethrows()
    {
        // Arrange
        var request = new CompletionRequest(Prompt: "fail");
        var exception = new OpenAIException(502,"Bad gateway");

        _innerMock
            .Setup(s => s.CompleteAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act & Assert
        await Should.ThrowAsync<OpenAIException>(() => _sut.CompleteAsync(request));

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_OpenAIException_DoesNotSwallowException()
    {
        // Arrange
        var request = new CompletionRequest(Prompt: "throw");
        var exception = new OpenAIException(429,"Rate limited");

        _innerMock
            .Setup(s => s.CompleteAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var thrown = await Should.ThrowAsync<OpenAIException>(() => _sut.CompleteAsync(request));
        thrown.StatusCode.ShouldBe(429);
        thrown.Message.ShouldBe("Rate limited");
    }

    [Fact]
    public async Task CompleteAsync_PassesCancellationToken_ToInner()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var request = new CompletionRequest(Prompt: "x");

        _innerMock
            .Setup(s => s.CompleteAsync(request, token))
            .ReturnsAsync(new CompletionResult("x", 1, "stop"));

        // Act
        await _sut.CompleteAsync(request, token);

        // Assert
        _innerMock.Verify(s => s.CompleteAsync(request, token), Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_UnexpectedException_DoesNotLogAsOpenAIError()
    {
        // Arrange – plain exception (not OpenAIException) must NOT be caught by the decorator
        var request = new CompletionRequest(Prompt: "unexpected");

        _innerMock
            .Setup(s => s.CompleteAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected"));

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(() => _sut.CompleteAsync(request));

        // Error-level logging should NOT have been called (only catches OpenAIException)
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}