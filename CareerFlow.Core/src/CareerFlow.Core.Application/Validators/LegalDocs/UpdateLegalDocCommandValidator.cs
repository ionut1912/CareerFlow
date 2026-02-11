using CareerFlow.Core.Application.CQRS.Legal.Command;
using FluentValidation;

namespace CareerFlow.Core.Application.Validators.LegalDocs;

public class UpdateLegalDocCommandValidator : AbstractValidator<UpdateLegalDocCommand>
{
    public UpdateLegalDocCommandValidator()
    {
        RuleFor(x => x.Type)
        .NotEmpty()
        .Must(type => type.IsValid())
        .WithMessage($"Type must be one of: {string.Join(", ", LegalDocValidationExtensions._allowedTypes)}");
    }
}
