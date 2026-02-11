using CareerFlow.Core.Application.CQRS.Accounts.Query;
using FluentValidation;

namespace CareerFlow.Core.Application.Validators.Account;

public class GetCurrentAccountQueryValidator : AbstractValidator<GetCurrentAccountQuery>
{
    public GetCurrentAccountQueryValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty().WithMessage("Username is required");
    }
}