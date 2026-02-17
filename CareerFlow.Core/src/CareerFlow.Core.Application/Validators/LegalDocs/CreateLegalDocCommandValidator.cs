using CareerFlow.Core.Application.CQRS.Legal.Command;
using FluentValidation;

namespace CareerFlow.Core.Application.Validators.LegalDocs;

public class CreateLegalDocCommandValidator : AbstractValidator<CreateLegalDocCommand>
{

    public CreateLegalDocCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Continutul este necesar");

        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Tipul este necesar")
            .Must(type => type.IsValidLegalDocType())
            .WithMessage($"Tipul trebuie sa fie una din urmatoarele: {string.Join(", ", LegalDocValidationExtensions.AllowedTypes)}");
    }
}
