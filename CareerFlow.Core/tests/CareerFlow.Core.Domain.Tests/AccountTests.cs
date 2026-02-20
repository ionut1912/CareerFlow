using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using Moq;
using Shouldly;

namespace CareerFlow.Core.Domain.Tests;

public class AccountTests
{
    [Fact]
    public void Account_ValidFields_CreatesAccount()
    {
        //Arrange
        var email = "testEmail";
        var password = "testPassword";
        var username = "testUsername";
        var name = "testName";

        //Act
        var account = Account.Create(email, password, username, name);

        //Assert
        account.ShouldNotBeNull();
        account.Email.ShouldBe(email);
        account.Password.ShouldBe(password);
        account.Username.ShouldBe(username);
        account.Name.ShouldBe(name);
        account.IsFounder.ShouldBeFalse();
        account.PrivacyPolicyAccepted.ShouldBeFalse();
        account.TermsAccepted.ShouldBeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Account_WhenEmailInvalid_ThrowsException(string email)
    {
        //Arrange
        var password = "testPassword";
        var username = "testUsername";
        var name = "testName";

        //Act
        var exception = Should.Throw<InvalidFieldException>(() =>
            Account.Create(email, password, username, name));

        //Assert
        exception.Message.ShouldBe("Email-ul este invalid");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Account_WhenPasswordInvalid_ThrowsException(string password)
    {
        //Arrange
        var email = "testEmail";
        var username = "testUsername";
        var name = "testName";

        //Act
        var exception = Should.Throw<InvalidFieldException>(() =>
            Account.Create(email, password, username, name));

        //Assert
        exception.Message.ShouldBe("Parola este ivalida");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Account_WhenUsernameInvalid_ThrowsException(string username)
    {
        //Arrange
        var email = "testEmail";
        var password = "testPassword";
        var name = "testName";

        //Act
        var exception = Should.Throw<InvalidFieldException>(() =>
            Account.Create(email, password, username, name));

        //Assert
        exception.Message.ShouldBe("Username-ul este invalid");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Account_WhenNameInvalid_ThrowsException(string name)
    {
        //Arrange
        var email = "testEmail";
        var password = "testPassword";
        var username = "testUsername";

        //Act
        var exception = Should.Throw<InvalidFieldException>(() =>
            Account.Create(email, password, username, name));

        //Assert
        exception.Message.ShouldBe("Numele este invalid");
    }

    [Fact]
    public void AccountHashPassword_ValidPassword_HashPasword()
    {
        //Arramge
        var email = "testEmail";
        var password = "testPassword";
        var username = "testUsername";
        var name = "testName";
        var account = Account.Create(email, password, username, name);

        var hashedPassword = "hashedPassword";
        var passwordServiceMock = new Mock<IPasswordService>();
        passwordServiceMock.Setup(x => x.HashPassword(account.Password))
            .Returns(hashedPassword);

        //Act
        account.HashPassword(passwordServiceMock.Object);

        //Assert
        account.Password.ShouldBe(hashedPassword);
        passwordServiceMock.Verify(x => x.HashPassword(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void AccountResetassword_ValidPassword_ChangesPassword()
    {
        //Arrange
        var email = "testEmail";
        var password = "testPassword";
        var username = "testUsername";
        var name = "testName";
        var account = Account.Create(email, password, username, name);
        var hashedPassword = "hashedPassword";
        var passwordServiceMock = new Mock<IPasswordService>();
        passwordServiceMock.Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns(hashedPassword);

        //Act
        account.ResetPassword("newPasword", passwordServiceMock.Object);

        //Assert
        account.Password.ShouldBe(hashedPassword);
        passwordServiceMock.Verify(x => x.HashPassword(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void Account_MarkAsFounder_IsFounderBecomeTrue()
    {
        //Arrange
        var email = "testEmail";
        var password = "testPassword";
        var username = "testUsername";
        var name = "testName";
        var account = Account.Create(email, password, username, name);

        //Act
        account.MarkAsFounder();

        //Assert
        account.ShouldNotBeNull();
        account.Email.ShouldBe(email);
        account.Password.ShouldBe(password);
        account.Username.ShouldBe(username);
        account.Name.ShouldBe(name);
        account.IsFounder.ShouldBeTrue();
        account.PrivacyPolicyAccepted.ShouldBeFalse();
        account.TermsAccepted.ShouldBeFalse();
    }


    [Fact]
    public void Account_AcceptTerms_TermsAcceptedBecomeTrue()
    {
        //Arrange
        var email = "testEmail";
        var password = "testPassword";
        var username = "testUsername";
        var name = "testName";
        var account = Account.Create(email, password, username, name);

        //Act
        account.AcceptTerms();

        //Assert
        account.ShouldNotBeNull();
        account.Email.ShouldBe(email);
        account.Password.ShouldBe(password);
        account.Username.ShouldBe(username);
        account.Name.ShouldBe(name);
        account.IsFounder.ShouldBeFalse();
        account.PrivacyPolicyAccepted.ShouldBeFalse();
        account.TermsAccepted.ShouldBeTrue();
    }

    [Fact]
    public void Account_AcceptPrivacyPolicy_PrivacyPolicyAcceptedBecomeTrue()
    {
        //Arrange
        var email = "testEmail";
        var password = "testPassword";
        var username = "testUsername";
        var name = "testName";
        var account = Account.Create(email, password, username, name);

        //Act
        account.AcceptPrivacyPolicy();

        //Assert
        account.ShouldNotBeNull();
        account.Email.ShouldBe(email);
        account.Password.ShouldBe(password);
        account.Username.ShouldBe(username);
        account.Name.ShouldBe(name);
        account.IsFounder.ShouldBeFalse();
        account.PrivacyPolicyAccepted.ShouldBeTrue();
        account.TermsAccepted.ShouldBeFalse();
    }
}