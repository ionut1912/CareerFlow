using CareerFlow.Core.Application.CQRS.Accounts.Command;
using CareerFlow.Core.Application.Validators.Account;
using FluentValidation.TestHelper;

namespace CareerFlow.Core.Application.Tests.Validators.Accounts;

public class CreateAccountCommandValidatorTests
{
    private readonly CreateAccountCommandValidator _validator;

    public CreateAccountCommandValidatorTests()
    {
        _validator = new CreateAccountCommandValidator();
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveErrors()
    {
        // Arrange
        var command =
            new CreateAccountCommand("test@email.com", "Password123!", "Password123!", "username", "Test Name");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_WhenEmailIsEmpty_ShouldHaveError(string email)
    {
        // Arrange: Create command with the invalid email and dummy data for others
        var command = new CreateAccountCommand(email, "pass", "pass", "user", "name");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email-ul este necesar");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("test@")]
    [InlineData("@test.com")]
    public void Validate_WhenEmailIsInvalidFormat_ShouldHaveError(string email)
    {
        // Arrange
        var command = new CreateAccountCommand(email, "pass", "pass", "user", "name");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email-ul are format invalid");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WhenPasswordIsEmpty_ShouldHaveError(string password)
    {
        // Arrange
        var command = new CreateAccountCommand("test@test.com", password, password, "user", "name");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Parola este necesara");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WhenUsernameIsEmpty_ShouldHaveError(string username)
    {
        // Arrange
        var command = new CreateAccountCommand("test@test.com", "testPassword", "testPassword", username, "name");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("Numele de utilizator este necesar");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WheNameIsEmpty_ShouldHaveError(string name)
    {
        // Arrange
        var command = new CreateAccountCommand("test@test.com", "testPassword", "testPassword", "test", name);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Numele este necesar");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WheConfirmPasswordIsEmpty_ShouldHaveError(string confirmPassword)
    {
        //Arrange
        var command = new CreateAccountCommand("test@test.com", "testPassword", confirmPassword, "test", "name");

        //Act
        var result = _validator.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
            .WithErrorMessage("Parola de confirmare este necesara");
    }
}