using CareerFlow.Core.Application.Mediatr.Accounts.Query;
using FluentValidation;

namespace CareerFlow.Core.Application.Validators.Account;

public class GetCurrentAccountQueryValidator : AbstractValidator<GetCurrentAccountQuery>
{
    public GetCurrentAccountQueryValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required");
    }
}