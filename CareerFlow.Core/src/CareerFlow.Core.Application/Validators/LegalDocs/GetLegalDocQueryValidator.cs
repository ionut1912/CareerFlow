using CareerFlow.Core.Application.CQRS.Legal.Query;
using FluentValidation;

namespace CareerFlow.Core.Application.Validators.LegalDocs;

public class GetLegalDocQueryValidator : AbstractValidator<GetLegalDocQuery>
{
    public GetLegalDocQueryValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Tipul este necesar")
            .Must(type => type.IsValidLegalDocType())
            .WithMessage(
                $"Tipul trebuie sa fie una din urmatoarele: {string.Join(", ", LegalDocValidationExtensions.AllowedTypes)}");
    }
}