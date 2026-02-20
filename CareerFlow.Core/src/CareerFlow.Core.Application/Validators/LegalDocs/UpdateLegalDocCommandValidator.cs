using CareerFlow.Core.Application.CQRS.Legal.Command;
using FluentValidation;

namespace CareerFlow.Core.Application.Validators.LegalDocs;

public class UpdateLegalDocCommandValidator : AbstractValidator<UpdateLegalDocCommand>
{
    public UpdateLegalDocCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Continutul este necesar");

        RuleFor(x => x.Type)
            .Must(type => type.IsValidLegalDocType())
            .WithMessage(
                $"Tipul trebuie sa fie una din urmatoarele: {string.Join(", ", LegalDocValidationExtensions.AllowedTypes)}");
    }
}