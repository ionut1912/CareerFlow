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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendEmailWithTemplateAsync_EmptyOrNullTo_ReturnsFalse(string? to)
    {
        // Arrange
        var model = new Dictionary<string, string>();

        // Act
        var result = await _sut.SendEmailWithTemplateAsync(to!, 1, model);

        // Assert
        result.ShouldBeFalse();

        _mailClientMock.Verify(x => x.SendTemplatedEmailAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<Dictionary<string, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task SendEmailWithTemplateAsync_ProviderReturnsSuccess_ReturnsTrue()
    {
        // Arrange
        var model = new Dictionary<string, string> { { "key", "value" } };

        _mailClientMock
            .Setup(x => x.SendTemplatedEmailAsync("to@example.com", 42, model))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.SendEmailWithTemplateAsync("to@example.com", 42, model);

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
                It.IsAny<Dictionary<string, string>>()
            ))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.SendEmailWithTemplateAsync("to@example.com", 1, model);

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
                It.IsAny<Dictionary<string, string>>()))
            .ThrowsAsync(new HttpRequestException("Provider down"));

        // Act
        var result = await _sut.SendEmailWithTemplateAsync("to@example.com", 1, model);

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
            .Setup(x => x.SendTemplatedEmailAsync(to, templateId, model))
            .ReturnsAsync(true);

        // Act
        await _sut.SendEmailWithTemplateAsync(to, templateId, model);

        // Assert
        _mailClientMock.Verify(x => x.SendTemplatedEmailAsync(to, templateId, model),
            Times.Once);
    }
}