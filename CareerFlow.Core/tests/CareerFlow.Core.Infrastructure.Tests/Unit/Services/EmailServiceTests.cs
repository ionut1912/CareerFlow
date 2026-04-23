using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Infrastructure.Services;
using CareerFlow.Core.Infrastructure.Tests.Unit.Setup;

using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Unit.Services;

public class EmailServiceTests
{
    private readonly FakeLogger<EmailService> _fakeLogger = new();
    private readonly Mock<IMailClient> _mailClientMock = new();
    private readonly EmailService _sut;

    public EmailServiceTests()
    {
        _sut = new EmailService(_mailClientMock.Object, _fakeLogger);
    }

    // ── Constructor ──────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullMailClient_ThrowsArgumentNullException() =>
        Should.Throw<ArgumentNullException>(() => new EmailService(null!, _fakeLogger));

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException() =>
        Should.Throw<ArgumentNullException>(() => new EmailService(_mailClientMock.Object, null!));

    // ── SendEmailWithTemplateAsync — invalid receiver ─────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendEmailWithTemplateAsync_WhenReceiverIsNullOrWhiteSpace_ReturnsFalse(string? receiver)
    {
        // Act
        bool result = await _sut.SendEmailWithTemplateAsync(receiver!, 1, []);

        // Assert
        result.ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendEmailWithTemplateAsync_WhenReceiverIsNullOrWhiteSpace_LogsError(string? receiver)
    {
        // Act
        await _sut.SendEmailWithTemplateAsync(receiver!, 1, []);

        // Assert
        FakeLogRecord record = _fakeLogger.Records.ShouldHaveSingleItem();
        record.Level.ShouldBe(LogLevel.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendEmailWithTemplateAsync_WhenReceiverIsNullOrWhiteSpace_DoesNotCallMailClient(string? receiver)
    {
        // Act
        await _sut.SendEmailWithTemplateAsync(receiver!, 1, []);

        // Assert
        _mailClientMock.Verify(
            c => c.SendTemplatedEmailAsync(It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<Dictionary<string, string>>()),
            Times.Never);
    }

    // ── SendEmailWithTemplateAsync — success path ─────────────────────────────

    [Fact]
    public async Task SendEmailWithTemplateAsync_WhenMailClientSucceeds_ReturnsTrue()
    {
        // Arrange
        _mailClientMock
            .Setup(c => c.SendTemplatedEmailAsync(It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(true);

        // Act
        bool result = await _sut.SendEmailWithTemplateAsync("user@example.com", 42, []);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task SendEmailWithTemplateAsync_WhenMailClientSucceeds_LogsInformationWithTemplateId()
    {
        // Arrange
        _mailClientMock
            .Setup(c => c.SendTemplatedEmailAsync(It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(true);

        // Act
        await _sut.SendEmailWithTemplateAsync("user@example.com", 42, []);

        // Assert
        FakeLogRecord record = _fakeLogger.Records.ShouldHaveSingleItem();
        record.Level.ShouldBe(LogLevel.Information);
        record.Message.ShouldContain("42");
    }

    [Fact]
    public async Task SendEmailWithTemplateAsync_WhenMailClientSucceeds_PassesCorrectArgumentsToClient()
    {
        // Arrange
        var placeholders = new Dictionary<string, string> { ["name"] = "Alice" };
        _mailClientMock
            .Setup(c => c.SendTemplatedEmailAsync(It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(true);

        // Act
        await _sut.SendEmailWithTemplateAsync("alice@example.com", 7, placeholders);

        // Assert
        _mailClientMock.Verify(
            c => c.SendTemplatedEmailAsync("alice@example.com", 7, placeholders),
            Times.Once);
    }

    // ── SendEmailWithTemplateAsync — provider failure path ────────────────────

    [Fact]
    public async Task SendEmailWithTemplateAsync_WhenMailClientReturnsFalse_ReturnsFalse()
    {
        // Arrange
        _mailClientMock
            .Setup(c => c.SendTemplatedEmailAsync(It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(false);

        // Act
        bool result = await _sut.SendEmailWithTemplateAsync("user@example.com", 1, []);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task SendEmailWithTemplateAsync_WhenMailClientReturnsFalse_LogsError()
    {
        // Arrange
        _mailClientMock
            .Setup(c => c.SendTemplatedEmailAsync(It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(false);

        // Act
        await _sut.SendEmailWithTemplateAsync("user@example.com", 1, []);

        // Assert
        FakeLogRecord record = _fakeLogger.Records.ShouldHaveSingleItem();
        record.Level.ShouldBe(LogLevel.Error);
    }

    // ── SendEmailWithTemplateAsync — exception path ───────────────────────────

    [Fact]
    public async Task SendEmailWithTemplateAsync_WhenInvalidOperationExceptionThrown_ReturnsFalse()
    {
        // Arrange
        _mailClientMock
            .Setup(c => c.SendTemplatedEmailAsync(It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<Dictionary<string, string>>()))
            .ThrowsAsync(new InvalidOperationException("provider error"));

        // Act
        bool result = await _sut.SendEmailWithTemplateAsync("user@example.com", 1, []);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task SendEmailWithTemplateAsync_WhenInvalidOperationExceptionThrown_LogsErrorWithTemplateId()
    {
        // Arrange
        _mailClientMock
            .Setup(c => c.SendTemplatedEmailAsync(It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<Dictionary<string, string>>()))
            .ThrowsAsync(new InvalidOperationException("provider error"));

        // Act
        await _sut.SendEmailWithTemplateAsync("user@example.com", 99, []);

        // Assert
        FakeLogRecord record = _fakeLogger.Records.ShouldHaveSingleItem();
        record.Level.ShouldBe(LogLevel.Error);
        record.Message.ShouldContain("99");
    }

    [Fact]
    public async Task SendEmailWithTemplateAsync_WhenUnexpectedExceptionThrown_PropagatesException()
    {
        // Arrange — only InvalidOperationException is caught; everything else must bubble up
        _mailClientMock
            .Setup(c => c.SendTemplatedEmailAsync(It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<Dictionary<string, string>>()))
            .ThrowsAsync(new HttpRequestException("network failure"));

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(() =>
            _sut.SendEmailWithTemplateAsync("user@example.com", 1, []));
    }
}
