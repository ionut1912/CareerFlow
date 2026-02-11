using CareerFlow.Core.Application.CQRS.Legal.Command;
using FluentValidation;

namespace CareerFlow.Core.Application.Validators.LegalDocs;

public class CreateLegalDocCommandValidator : AbstractValidator<CreateLegalDocCommand>
{

    public CreateLegalDocCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.");

        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(type => type.IsValid())
            .WithMessage($"Type must be one of: {string.Join(", ", LegalDocValidationExtensions._allowedTypes)}");
    }
}
