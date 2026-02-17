using CareerFlow.Core.Application.CQRS.Legal.Command;
using CareerFlow.Core.Application.Validators.LegalDocs;
using FluentValidation.TestHelper;

namespace CareerFlow.Core.Application.Tests.Validators.Legal;

public class UpdateLegalDocCommandValidatorTests
{
    private readonly UpdateLegalDocCommandValidator _validator;

    public UpdateLegalDocCommandValidatorTests()
    {
        _validator = new UpdateLegalDocCommandValidator();
    }

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveErrors()
    {
        //Arrange
        var command = new UpdateLegalDocCommand("testContetn", "PrivacyPolicy");

        //Act
        var result = _validator.TestValidate(command);

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
        var command = new UpdateLegalDocCommand(contnent, "PrivacyPolicy");

        //Act
        var result = _validator.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Continutul este necesar");
    }

    [Theory]
    [InlineData("privacypollicy")]
    [InlineData("test")]
    public void Validate_WhenTypeIsInvalid_ShouldHaveErrors(string type)
    {
        //Arrange
        var command = new UpdateLegalDocCommand("testContent", type);

        //Act
        var result = _validator.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Type)
             .WithErrorMessage($"Tipul trebuie sa fie una din urmatoarele: {string.Join(", ", LegalDocValidationExtensions.AllowedTypes)}");
    }
}
