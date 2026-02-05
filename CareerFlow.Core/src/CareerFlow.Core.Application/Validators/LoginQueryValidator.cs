using CareerFlow.Core.Application.Mediatr.Accounts.Query;
using FluentValidation;

namespace CareerFlow.Core.Application.Validators;

public class LoginQueryValidator : AbstractValidator<LoginQuery>
{
    public LoginQueryValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
    }
}