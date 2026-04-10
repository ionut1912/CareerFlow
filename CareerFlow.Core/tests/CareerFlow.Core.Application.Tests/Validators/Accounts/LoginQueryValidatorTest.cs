using CareerFlow.Core.Application.CQRS.Accounts.Queries;
using CareerFlow.Core.Application.Validators.Account;
using FluentValidation.TestHelper;

namespace CareerFlow.Core.Application.Tests.Validators.Accounts;

public class LoginQueryValidatorTest
{
    private readonly LoginQueryValidator _validator;

    public LoginQueryValidatorTest()
    {
        _validator = new LoginQueryValidator();
    }

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveErrors()
    {
        // Arrange
        var query = new LoginQuery("test@email.com", "Password123!");

        // Act
        var result = _validator.TestValidate(query);

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
        var query = new LoginQuery(email, "pass");

        // Act
        var result = _validator.TestValidate(query);

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
        var query = new LoginQuery(email, "pass");

        // Act
        var result = _validator.TestValidate(query);

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
        var query = new LoginQuery("test@test.com", password);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Parola este necesara");
    }
}