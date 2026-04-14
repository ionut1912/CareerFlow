using CareerFlow.Core.Application.CQRS.Courses.Commands;
using FluentValidation;

namespace CareerFlow.Core.Application.Validators.Course;

public class FinishChapterCommandValidator : AbstractValidator<FinishChapterCommand>
{
    public FinishChapterCommandValidator()
    {
        RuleFor(command => command.CourseId).NotEmpty()
            .WithMessage("CourseId este necesar");

        RuleFor(command => command.ChapterId)
            .NotEmpty()
            .WithMessage("ChapterId este necesar");
    }
}