using CareerFlow.Core.Application.CQRS.UserProfiles.Commands;
using CareerFlow.Core.Application.Validators.UserProfiles;
using FluentValidation.TestHelper;

namespace CareerFlow.Core.Application.Tests.Validators.UserProfiles;

public class CreateUserProfileCommandValidatorTests
{
    private readonly CreateUserProfileCommandValidator _validator = new();

    [Fact]
    public void ValidateValidCommandShouldNotHaveValidationError()
    {
        //Arrange
        var command = new CreateUserProfileCommand(Guid.NewGuid(), "Visual", ["Student"], "test");

        //Act
        TestValidationResult<CreateUserProfileCommand>? result = _validator.TestValidate(command);

        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_EmptyLearningType_ShouldHaveError(string? learningType)
    {
        //Arrange
        var command = new CreateUserProfileCommand(Guid.NewGuid(), learningType!, ["Student"], "test");

        //Act
        TestValidationResult<CreateUserProfileCommand>? result = _validator.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.LearningType)
            .WithErrorMessage("Tipul de invatare este necesar.");
    }

    [Fact]
    public void Validate_InvalidLearningType_ShouldHaveError()
    {
        //Arrange
        var command = new CreateUserProfileCommand(Guid.NewGuid(), "invalidType", ["Student"], "test");

        //Act
        TestValidationResult<CreateUserProfileCommand>? result = _validator.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.LearningType)
            .WithErrorMessage(
                $"Tipul de invatare trebuie sa fie unul dinte: {string.Join(", ", CreateUserProfileCommandValidator.ValidLearningTypes)}.");
    }

    [Theory]
    [MemberData(nameof(EmptyUserTypeData))]
    public void Validate_EmptyUserType_ShouldHaveError(string[]? userTypes)
    {
        //Arrange
        var command = new CreateUserProfileCommand(Guid.NewGuid(), "Visual", userTypes?.ToList()!, "test");

        //Act
        TestValidationResult<CreateUserProfileCommand>? result = _validator.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.UserTypes)
            .WithErrorMessage("Cel putin un tip pentru user este necesar.");
    }

    [Fact]
    public void Validate_InvalidTypesCount_ShouldHaveError()
    {
        //Arrange
        var command = new CreateUserProfileCommand(Guid.NewGuid(), "Visual",
            ["Student", "JobSearcher", "HobbyLearner", "test"], "test");

        //Act
        TestValidationResult<CreateUserProfileCommand>? result = _validator.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.UserTypes)
            .WithErrorMessage("Prea multe tipuri.");
    }

    [Fact]
    public void Validate_InvalidUserType_ShouldHaveError()
    {
        //Arrange
        var command = new CreateUserProfileCommand(Guid.NewGuid(), "Visual", ["test"], "test");

        //Act
        TestValidationResult<CreateUserProfileCommand>? result = _validator.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.UserTypes)
            .WithErrorMessage(
                $"Fiecare tip de user trebuie sa fie unul dintre: {string.Join(", ", CreateUserProfileCommandValidator.ValidUserTypes)}.");
    }

    [Fact]
    public void Validate_DuplicateUserType_ShouldHaveError()
    {
        //Arrange
        var command = new CreateUserProfileCommand(Guid.NewGuid(), "Visual", ["Student", "Student"], "test");

        //Act
        TestValidationResult<CreateUserProfileCommand>? result = _validator.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.UserTypes)
            .WithErrorMessage("Tipul user nu trebuie sa contina duplicate");
    }

    [Fact]
    public void Validate_DomainLengthGreatherThan100_ShouldHaveError()
    {
        //Arrange
        var command = new CreateUserProfileCommand(Guid.NewGuid(), "Visual", ["Student"],
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

        //Act
        TestValidationResult<CreateUserProfileCommand>? result = _validator.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Domain)
            .WithErrorMessage("Domeniul nu trebuie sa aiba mai mult de 100 caractere.");
    }

    public static TheoryData<string[]?> EmptyUserTypeData() =>
    [
        [],
        null
    ];
}
