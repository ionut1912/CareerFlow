using CareerFlow.Core.Application.CQRS.Courses.Commands;
using CareerFlow.Core.Application.Validators.Course;
using FluentValidation.TestHelper;
using Shouldly;

namespace CareerFlow.Core.Application.Tests.Validators.Course;

public class GenerateCourseCommandValidatorTests
{
    private readonly GenerateCourseCommandValidator _sut = new();
 
    [Fact]
    public void Validate_ValidTopic_PassesWithNoErrors()
    {
        var command = new GenerateCourseCommand(Guid.NewGuid(), "C# Advanced");
 
        var result = _sut.TestValidate(command);
 
        result.ShouldNotHaveAnyValidationErrors();
    }
 
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_EmptyOrNullTopic_HasErrorWithRomanianMessage(string? topic)
    {
        var command = new GenerateCourseCommand(Guid.NewGuid(), topic!);
 
        var result = _sut.TestValidate(command);
 
        result.ShouldHaveValidationErrorFor(x => x.Topic)
            .WithErrorMessage("Topic este necesar");
    }
 
    [Theory]
    [InlineData("A")]
    [InlineData("Very long topic about advanced data structures and algorithms in C#")]
    public void Validate_NonEmptyTopic_PassesValidation(string topic)
    {
        var command = new GenerateCourseCommand(Guid.NewGuid(), topic);
 
        var result = _sut.TestValidate(command);
 
        result.ShouldNotHaveAnyValidationErrors();
    }
 
    [Fact]
    public void Validate_OnlyErrorIsForTopic_WhenTopicEmpty()
    {
        var command = new GenerateCourseCommand(Guid.NewGuid(), "");
 
        var result = _sut.TestValidate(command);
 
        result.Errors.Count.ShouldBe(1);
        result.Errors[0].PropertyName.ShouldBe(nameof(command.Topic));
    }
}