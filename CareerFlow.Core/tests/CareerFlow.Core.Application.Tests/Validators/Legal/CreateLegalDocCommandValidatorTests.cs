using CareerFlow.Core.Application.CQRS.Legal.Command;
using CareerFlow.Core.Application.Validators.LegalDocs;
using FluentValidation.TestHelper;

namespace CareerFlow.Core.Application.Tests.Validators.Legal;

public class CreateLegalDocCommandValidatorTests
{
    private readonly CreateLegalDocCommandValidator _validator;

    public CreateLegalDocCommandValidatorTests()
    {
        _validator = new CreateLegalDocCommandValidator();
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveErrors()
    {
        //Arrange
        var query = new CreateLegalDocCommand("test", "PrivacyPolicy");

        //Act
        var result = _validator.TestValidate(query);

        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("  ")]
    public void Validate_WhenContentIsEmpty_ShouldHaveError(string contnent)
    {
        //Arrange
        var command = new CreateLegalDocCommand(contnent, "PrivacyPolicy");

        //Act
        var result = _validator.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Continutul este necesar");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("  ")]
    public void Validate_WhenTypeIsEmpty_ShouldHaveErrors(string type)
    {
        //Arrange
        var command = new CreateLegalDocCommand("testContent", type);

        //Act
        var result = _validator.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorMessage("Tipul este necesar");
    }

    [Theory]
    [InlineData("privacypollicy")]
    [InlineData("test")]
    public void Validate_WhenTypeIsInvalid_ShouldHaveErrors(string type)
    {
        //Arrange
        var command = new CreateLegalDocCommand("testContent", type);

        //Act
        var result = _validator.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Type)
             .WithErrorMessage($"Tipul trebuie sa fie una din urmatoarele: {string.Join(", ", LegalDocValidationExtensions.AllowedTypes)}");
    }
}
