using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using Moq;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Domain.Tests.Entities;

public class AccountEntityTests
{
    private static Account CreateValidAccount(
        string email = "test@test.com",
        string password = "Password1!",
        string username = "username",
        string name = "Full Name") =>
        Account.Create(email, password, username, name);

    [Fact]
    public void Create_ValidParameters_ReturnsAccount()
    {
        Account account = CreateValidAccount();

        account.ShouldNotBeNull();
        account.Email.ShouldBe("test@test.com");
        account.Password.ShouldBe("Password1!");
        account.Username.ShouldBe("username");
        account.Name.ShouldBe("Full Name");
    }

    [Fact]
    public void Create_ValidParameters_SetsDefaultFlagsToFalse()
    {
        Account account = CreateValidAccount();

        account.IsFounder.ShouldBeFalse();
        account.TermsAccepted.ShouldBeFalse();
        account.PrivacyPolicyAccepted.ShouldBeFalse();
    }

    [Fact]
    public void Create_ValidParameters_SetsCreatedAtToNow()
    {
        DateTime before = DateTime.UtcNow.AddSeconds(-1);

        Account account = CreateValidAccount();

        account.CreatedAt.ShouldBeInRange(before, DateTime.UtcNow.AddSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_InvalidEmail_ThrowsInvalidFieldException(string? email)
    {
        Should.Throw<InvalidFieldException>(() =>
            Account.Create(email!, "Password1!", "username", "Name"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_InvalidPassword_ThrowsInvalidFieldException(string? password)
    {
        Should.Throw<InvalidFieldException>(() =>
            Account.Create("test@test.com", password!, "username", "Name"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_InvalidUsername_ThrowsInvalidFieldException(string? username)
    {
        Should.Throw<InvalidFieldException>(() =>
            Account.Create("test@test.com", "Password1!", username!, "Name"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_InvalidName_ThrowsInvalidFieldException(string? name)
    {
        Should.Throw<InvalidFieldException>(() =>
            Account.Create("test@test.com", "Password1!", "username", name!));
    }

    [Fact]
    public void HashPassword_ValidService_UpdatesPasswordToHash()
    {

        Account account = CreateValidAccount();
        var passwordService = new Mock<IPasswordService>();
        passwordService.Setup(p => p.HashPassword("Password1!")).Returns("hashed_password");

        account.HashPassword(passwordService.Object);

        account.Password.ShouldBe("hashed_password");
    }

    [Fact]
    public void ResetPassword_ValidService_UpdatesPassword()
    {
        Account account = CreateValidAccount();
        var passwordService = new Mock<IPasswordService>();
        passwordService.Setup(p => p.HashPassword("NewPassword!")).Returns("new_hashed");

        account.ResetPassword("NewPassword!", passwordService.Object);

        account.Password.ShouldBe("new_hashed");
    }

    [Fact]
    public void ResetPassword_SetsUpdatedAtToNow()
    {
        Account account = CreateValidAccount();
        var passwordService = new Mock<IPasswordService>();
        passwordService.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("h");
        DateTime before = DateTime.UtcNow.AddSeconds(-1);

        account.ResetPassword("NewPassword!", passwordService.Object);

        ((DateTime)account.UpdatedAt!).ShouldBeGreaterThanOrEqualTo(before);
    }

    [Fact]
    public void MarkAsFounder_SetsIsFounderTrue()
    {
        Account account = CreateValidAccount();

        account.MarkAsFounder();

        account.IsFounder.ShouldBeTrue();
    }

    [Fact]
    public void MarkAsFounder_SetsUpdatedAtToNow()
    {
        Account account = CreateValidAccount();
        DateTime before = DateTime.UtcNow.AddSeconds(-1);

        account.MarkAsFounder();

        ((DateTime)account.UpdatedAt!).ShouldBeGreaterThanOrEqualTo(before);
    }

    [Fact]
    public void AcceptTerms_SetsTermsAcceptedTrue()
    {
        Account account = CreateValidAccount();

        account.AcceptTerms();

        account.TermsAccepted.ShouldBeTrue();
    }

    [Fact]
    public void AcceptTerms_SetsUpdatedAtToNow()
    {
        Account account = CreateValidAccount();
        DateTime before = DateTime.UtcNow.AddSeconds(-1);

        account.AcceptTerms();

        ((DateTime)account.UpdatedAt!).ShouldBeGreaterThanOrEqualTo(before);
    }

    [Fact]
    public void AcceptPrivacyPolicy_SetsPrivacyPolicyAcceptedTrue()
    {
        Account account = CreateValidAccount();

        account.AcceptPrivacyPolicy();

        account.PrivacyPolicyAccepted.ShouldBeTrue();
    }

    [Fact]
    public void AcceptPrivacyPolicy_SetsUpdatedAtToNow()
    {
        Account account = CreateValidAccount();
        DateTime before = DateTime.UtcNow.AddSeconds(-1);

        account.AcceptPrivacyPolicy();

        ((DateTime)account.UpdatedAt!).ShouldBeGreaterThanOrEqualTo(before);
    }

    [Fact]
    public void GenerateResetPasswordToken_ValidToken_StoresHashedToken()
    {
        Account account = CreateValidAccount();
        var passwordService = new Mock<IPasswordService>();
        passwordService.Setup(p => p.HashPassword("raw-token")).Returns("hashed-token");

        account.GenerateResetPasswordToken("raw-token", passwordService.Object);

        account.ResetPasswordToken.ShouldBe("hashed-token");
    }

    [Fact]
    public void GenerateResetPasswordToken_SetsExpiryOneHourFromNow()
    {
        Account account = CreateValidAccount();
        var passwordService = new Mock<IPasswordService>();
        passwordService.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("h");
        DateTime expectedExpiry = DateTime.UtcNow.AddMinutes(59);

        account.GenerateResetPasswordToken("raw-token", passwordService.Object);

        account.ResetPasswordTokenExpiresAt.ShouldBeGreaterThan(expectedExpiry);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GenerateResetPasswordToken_InvalidToken_ThrowsInvalidFieldException(string? token)
    {
        Account account = CreateValidAccount();
        var passwordService = new Mock<IPasswordService>();

        Should.Throw<InvalidFieldException>(() =>
            account.GenerateResetPasswordToken(token!, passwordService.Object));
    }

    [Fact]
    public void ResetPasswordTokenAndExpiry_ClearsTokenToEmptyString()
    {
        Account account = CreateValidAccount();
        var passwordService = new Mock<IPasswordService>();
        passwordService.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("h");
        account.GenerateResetPasswordToken("raw-token", passwordService.Object);

        account.ResetPasswordTokenAndExpiry();

        account.ResetPasswordToken.ShouldBe(string.Empty);
    }

    [Fact]
    public void SetResetPasswordExpiresAt_SetsExpiry()
    {
        Account account = CreateValidAccount();
        DateTime expiry = DateTime.UtcNow.AddHours(2);

        account.SetResetPasswordExpiresAt(expiry);

        account.ResetPasswordTokenExpiresAt.ShouldBe(expiry);
    }

    [Fact]
    public void Create_ReturnsUniqueIds_ForMultipleAccounts()
    {
        Account a1 = CreateValidAccount("a@a.com");
        Account a2 = CreateValidAccount("b@b.com");

        a1.Id.ShouldNotBe(a2.Id);
    }
}
