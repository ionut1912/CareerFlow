using CareerFlow.Core.Application.CQRS.Courses.Commands;
using CareerFlow.Core.Domain.Constants;
using FluentValidation;

namespace CareerFlow.Core.Application.Validators.Course;

public class UploadCourseDocumentCommandValidator : AbstractValidator<UploadCourseDocumentCommand>
{
    public UploadCourseDocumentCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Titlul este necesar");

        RuleFor(x => x.Files)
            .NotEmpty()
            .WithMessage("Files sunt necesare")
            .Must(files => files is null || files.Count <= CourseConstants.MaxFiles)
            .WithMessage($"Numărul maxim de fișiere este {CourseConstants.MaxFiles}");

        RuleForEach(x => x.Files)
            .ChildRules(file =>
            {
                file.RuleFor(f => f.Content)
                    .Must(s => s.Length > 0)
                    .WithMessage("Fișierul nu poate fi gol");
            });
    }
}