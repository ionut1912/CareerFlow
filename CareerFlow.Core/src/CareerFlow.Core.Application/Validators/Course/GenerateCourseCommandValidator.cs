using CareerFlow.Core.Application.CQRS.Courses.Commands;
using FluentValidation;

namespace CareerFlow.Core.Application.Validators.Course;

public class GenerateCourseCommandValidator:AbstractValidator<GenerateCourseCommand>
{
    public GenerateCourseCommandValidator()
    {
        RuleFor(x=>x.Topic)
            .NotEmpty()
            .WithMessage("Topic este necesar");
    }
}