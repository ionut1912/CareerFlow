using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Unit.Services;

public class EmailServiceTests
{
    private readonly Mock<ILogger<EmailService>> _loggerMock;
    private readonly Mock<IMailClient> _mailClientMock;
    private readonly EmailService _sut;

    public EmailServiceTests()
    {
        _mailClientMock = new Mock<IMailClient>();
        _loggerMock = new Mock<ILogger<EmailService>>();

        // Use .Object to access the actual implementation of the interface
        _sut = new EmailService(_mailClientMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void Constructor_NullMailClient_ThrowsArgumentNullException()
    {
        // Assert
        Should.Throw<ArgumentNullException>(() => new EmailService(null!, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Assert
        Should.Throw<ArgumentNullException>(() => new EmailService(_mailClientMock.Object, null!));
    }

    [Fact]
    public async Task SendEmailWithTemplateAsync_CancelledToken_ReturnsFalse()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var model = new Dictionary<string, string>();

        // Act
        var result = await _sut.SendEmailWithTemplateAsync("test@example.com", 1, model, cts.Token);

        // Assert
        result.ShouldBeFalse();

        // Verify that the method was never called
        _mailClientMock.Verify(x => x.SendTemplatedEmailAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendEmailWithTemplateAsync_EmptyOrNullTo_ReturnsFalse(string? to)
    {
        // Arrange
        var model = new Dictionary<string, string>();

        // Act
        var result = await _sut.SendEmailWithTemplateAsync(to!, 1, model, CancellationToken.None);

        // Assert
        result.ShouldBeFalse();

        _mailClientMock.Verify(x => x.SendTemplatedEmailAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SendEmailWithTemplateAsync_ProviderReturnsSuccess_ReturnsTrue()
    {
        // Arrange
        var model = new Dictionary<string, string> { { "key", "value" } };

        _mailClientMock
            .Setup(x => x.SendTemplatedEmailAsync("to@example.com", 42, model, CancellationToken.None))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.SendEmailWithTemplateAsync("to@example.com", 42, model, CancellationToken.None);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task SendEmailWithTemplateAsync_ProviderReturnsFailure_ReturnsFalse()
    {
        // Arrange
        var model = new Dictionary<string, string>();

        _mailClientMock
            .Setup(x => x.SendTemplatedEmailAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.SendEmailWithTemplateAsync("to@example.com", 1, model, CancellationToken.None);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task SendEmailWithTemplateAsync_ProviderThrowsException_ReturnsFalse()
    {
        // Arrange
        var model = new Dictionary<string, string>();

        _mailClientMock
            .Setup(x => x.SendTemplatedEmailAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Provider down"));

        // Act
        var result = await _sut.SendEmailWithTemplateAsync("to@example.com", 1, model, CancellationToken.None);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task SendEmailWithTemplateAsync_ValidInput_CallsProviderWithCorrectArguments()
    {
        // Arrange
        const string to = "user@domain.com";
        const int templateId = 99;
        var model = new Dictionary<string, string> { { "name", "Alice" } };

        _mailClientMock
            .Setup(x => x.SendTemplatedEmailAsync(to, templateId, model, CancellationToken.None))
            .ReturnsAsync(true);

        // Act
        await _sut.SendEmailWithTemplateAsync(to, templateId, model, CancellationToken.None);

        // Assert
        _mailClientMock.Verify(x => x.SendTemplatedEmailAsync(to, templateId, model, CancellationToken.None),
            Times.Once);
    }
}