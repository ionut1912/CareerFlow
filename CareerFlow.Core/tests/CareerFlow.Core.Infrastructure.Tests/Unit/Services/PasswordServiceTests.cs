using CareerFlow.Core.Infrastructure.Services;

using Shouldly;

using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Unit.Services;

public class PasswordServiceTests
{
    private readonly PasswordService _sut = new();

    [Fact]
    public void HashPassword_ValidPassword_ReturnsNonEmptyHash()
    {
        // Arrange
        const string password = "SuperSecret123!";

        // Act
        string hash = _sut.HashPassword(password);

        // Assert
        hash.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void HashPassword_ValidPassword_ReturnsBcryptFormattedHash()
    {
        // Arrange
        const string password = "AnotherPassword";

        // Act
        string hash = _sut.HashPassword(password);

        // Assert
        hash.ShouldStartWith("$2");
    }

    [Fact]
    public void HashPassword_SamePasswordTwice_ProducesDifferentHashes()
    {
        // Arrange
        const string password = "SamePassword";

        // Act
        string hash1 = _sut.HashPassword(password);
        string hash2 = _sut.HashPassword(password);

        // Assert
        hash1.ShouldNotBe(hash2);
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        const string password = "CorrectHorseBatteryStaple";
        string hash = _sut.HashPassword(password);

        // Act
        bool result = _sut.VerifyPassword(password, hash);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        // Arrange
        string hash = _sut.HashPassword("CorrectPassword");

        // Act
        bool result = _sut.VerifyPassword("WrongPassword", hash);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void VerifyPassword_EmptyPassword_ReturnsFalse()
    {
        // Arrange
        string hash = _sut.HashPassword("realpassword");

        // Act
        bool result = _sut.VerifyPassword(string.Empty, hash);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void HashAndVerify_RoundTrip_Succeeds()
    {
        // Arrange
        const string password = "RoundTripPa$$w0rd";

        // Act
        string hash = _sut.HashPassword(password);
        bool verified = _sut.VerifyPassword(password, hash);

        // Assert
        verified.ShouldBeTrue();
    }
}
