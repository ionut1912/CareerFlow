using CareerFlow.Core.Application.CQRS.Legal.Query;
using FluentValidation;

namespace CareerFlow.Core.Application.Validators.LegalDocs;

public class GetLegalDocQueryValidator : AbstractValidator<GetLegalDocQuery>
{
    public GetLegalDocQueryValidator()
    {
        RuleFor(x => x.Type)
        .NotEmpty()
        .Must(type => type.IsValid())
        .WithMessage($"Type must be one of: {string.Join(", ", LegalDocValidationExtensions._allowedTypes)}");
    }
}
