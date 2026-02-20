using CareerFlow.Core.Application.CQRS.Legal.Query;
using CareerFlow.Core.Application.Validators.LegalDocs;
using FluentValidation.TestHelper;

namespace CareerFlow.Core.Application.Tests.Validators.Legal;

public class GetLegalDocQueryValidatorTests
{
    private readonly GetLegalDocQueryValidator _validator;

    public GetLegalDocQueryValidatorTests()
    {
        _validator = new GetLegalDocQueryValidator();
    }

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveErrors()
    {
        //Arrange
        var query = new GetLegalDocQuery("PrivacyPolicy");

        //Act
        var result = _validator.TestValidate(query);

        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("  ")]
    public void Validate_WhenTypeIsEmpty_ShouldHaveErrors(string type)
    {
        //Arrange
        var query = new GetLegalDocQuery(type);

        //Act
        var result = _validator.TestValidate(query);

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
        var query = new GetLegalDocQuery(type);

        //Act
        var result = _validator.TestValidate(query);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorMessage(
                $"Tipul trebuie sa fie una din urmatoarele: {string.Join(", ", LegalDocValidationExtensions.AllowedTypes)}");
    }
}