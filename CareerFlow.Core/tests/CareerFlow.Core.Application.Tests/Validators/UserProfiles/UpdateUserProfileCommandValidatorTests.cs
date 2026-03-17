using CareerFlow.Core.Application.CQRS.UserProfiles.Commands;
using CareerFlow.Core.Application.Validators.UserProfiles;
using FluentValidation.TestHelper;

namespace CareerFlow.Core.Application.Tests.Validators.UserProfiles;

public class UpdateUserProfileCommandValidatorTests
{
    private readonly UpdateUserProfileCommandValidator _validator;

    public UpdateUserProfileCommandValidatorTests()
    {
        _validator = new UpdateUserProfileCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationError()
    {
        //Arrange
        var command = new UpdateUserProfileCommand(Guid.NewGuid(), "Visual", ["Student"], "test");

        //Act
        var result = _validator.TestValidate(command);

        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }


    [Fact]
    public void Validate_InvalidLearningType_ShouldHaveError()
    {
        //Arrange
        var command = new UpdateUserProfileCommand(Guid.NewGuid(), "invalidType", ["Student"], "test");

        //Act
        var result = _validator.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.LearningType)
            .WithErrorMessage(
                $"Tipul de invatare trebuie sa fie unul dinte: {string.Join(", ", CreateUserProfileCommandValidator.ValidLearningTypes)}.");
    }


    [Fact]
    public void Validate_InvalidTypesCount_ShouldHaveError()
    {
        //Arrange
        var command = new UpdateUserProfileCommand(Guid.NewGuid(), "Visual",
            ["Student", "JobSearcher", "HobbyLearner", "test"], "test");

        //Act
        var result = _validator.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.UserTypes)
            .WithErrorMessage("Prea multe tipuri.");
    }

    [Fact]
    public void Validate_InvalidUserType_ShouldHaveError()
    {
        //Arrange
        var command = new UpdateUserProfileCommand(Guid.NewGuid(), "Visual", ["test"], "test");

        //Act
        var result = _validator.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.UserTypes)
            .WithErrorMessage(
                $"Fiecare tip de user trebuie sa fie unul dintre: {string.Join(", ", CreateUserProfileCommandValidator.ValidUserTypes)}.");
    }

    [Fact]
    public void Validate_DuplicateUserType_ShouldHaveError()
    {
        //Arrange
        var command = new UpdateUserProfileCommand(Guid.NewGuid(), "Visual", ["Student", "Student"], "test");

        //Act
        var result = _validator.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.UserTypes)
            .WithErrorMessage("Tipul user nu trebuie sa contina duplicate");
    }

    [Fact]
    public void Validate_DomainLengthGreatherThan100_ShouldHaveError()
    {
        //Arrange
        var command = new UpdateUserProfileCommand(Guid.NewGuid(), "Visual", ["Student"],
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

        //Act
        var result = _validator.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Domain)
            .WithErrorMessage("Domeniul nu trebuie sa aiba mai mult de 100 caractere.");
    }
}