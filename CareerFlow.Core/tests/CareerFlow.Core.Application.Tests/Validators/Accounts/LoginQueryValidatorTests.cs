using CareerFlow.Core.Application.CQRS.Accounts.Queries;
using CareerFlow.Core.Application.Validators.Account;
using FluentValidation.TestHelper;

namespace CareerFlow.Core.Application.Tests.Validators.Accounts;

public class LoginQueryValidatorTests
{
    private readonly LoginQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveErrors()
    {
        // Arrange
        var query = new LoginQuery("test@email.com", "Password123!");

        // Act
        TestValidationResult<LoginQuery>? result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)] // This triggers xUnit1012 if the parameter isn't nullable
    [InlineData("   ")]
    public void Validate_WhenEmailIsEmpty_ShouldHaveError(string? email)
    {
        // Arrange
        // We use ! here because the Query likely expects a non-null string,
        // but we are intentionally passing null to test validation.
        var query = new LoginQuery(email!, "pass");

        // Act
        TestValidationResult<LoginQuery>? result = _validator.TestValidate(query);

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
        TestValidationResult<LoginQuery>? result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email-ul are format invalid");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WhenPasswordIsEmpty_ShouldHaveError(string? password)
    {
        // Arrange
        var query = new LoginQuery("test@test.com", password!);

        // Act
        TestValidationResult<LoginQuery>? result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Parola este necesara");
    }
}
