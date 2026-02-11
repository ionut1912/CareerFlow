using CareerFlow.Core.Application.CQRS.Accounts.Query;
using FluentValidation;

namespace CareerFlow.Core.Application.Validators.Account;

public class LoginQueryValidator : AbstractValidator<LoginQuery>
{
    public LoginQueryValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Username is required");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
    }
}