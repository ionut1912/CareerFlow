using CareerFlow.Core.Application.CQRS.Courses.Commands;
using FluentValidation;

namespace CareerFlow.Core.Application.Validators.Course;

public class UploadCourseDocumentCommandValidator:AbstractValidator<UploadCourseDocumentCommand>
{
    public UploadCourseDocumentCommandValidator()
    {
        RuleFor(x=>x.Title)
            .NotEmpty()
            .WithMessage("Titlul este necesar");
        
        RuleFor(x=> x.Files)
            .NotEmpty()
            .WithMessage("Files sunt necesare");
    }
}