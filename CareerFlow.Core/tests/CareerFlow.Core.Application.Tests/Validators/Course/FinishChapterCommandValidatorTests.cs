using CareerFlow.Core.Application.CQRS.Courses.Commands;
using CareerFlow.Core.Application.Validators.Course;
using FluentValidation.TestHelper;
using Shouldly;

namespace CareerFlow.Core.Application.Tests.Validators.Course;

public class FinishChapterCommandValidatorTests
{
    private readonly FinishChapterCommandValidator _sut = new();
 
    [Fact]
    public void Validate_AllValidIds_PassesWithNoErrors()
    {
        var command = new FinishChapterCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
 
        var result = _sut.TestValidate(command);
 
        result.ShouldNotHaveAnyValidationErrors();
    }
 
    [Fact]
    public void Validate_EmptyCourseId_HasErrorWithRomanianMessage()
    {
        var command = new FinishChapterCommand(Guid.NewGuid(), Guid.Empty, Guid.NewGuid());
 
        var result = _sut.TestValidate(command);
 
        result.ShouldHaveValidationErrorFor(x => x.CourseId)
            .WithErrorMessage("CourseId este necesar");
    }
 
    [Fact]
    public void Validate_EmptyChapterId_HasErrorWithRomanianMessage()
    {
        var command = new FinishChapterCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty);
 
        var result = _sut.TestValidate(command);
 
        result.ShouldHaveValidationErrorFor(x => x.ChapterId)
            .WithErrorMessage("ChapterId este necesar");
    }
 
    [Fact]
    public void Validate_BothIdsEmpty_HasTwoErrors()
    {
        var command = new FinishChapterCommand(Guid.NewGuid(), Guid.Empty, Guid.Empty);
 
        var result = _sut.TestValidate(command);
 
        result.Errors.Count.ShouldBe(2);
        result.ShouldHaveValidationErrorFor(x => x.CourseId);
        result.ShouldHaveValidationErrorFor(x => x.ChapterId);
    }
 
    [Fact]
    public void Validate_UserIdEmpty_DoesNotValidateUserId()
    {
        var command = new FinishChapterCommand(Guid.Empty, Guid.NewGuid(), Guid.NewGuid());
 
        var result = _sut.TestValidate(command);
 
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }
}