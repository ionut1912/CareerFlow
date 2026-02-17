using CareerFlow.Core.Application.CQRS.Accounts.Query;
using FluentValidation;

namespace CareerFlow.Core.Application.Validators.Account;

public class LoginQueryValidator : AbstractValidator<LoginQuery>
{
    public LoginQueryValidator()
    {
        RuleFor(a => a.Email).NotEmpty().WithMessage("Email-ul este necesar")
           .EmailAddress().WithMessage("Email-ul are format invalid");
        RuleFor(a => a.Password).NotEmpty().WithMessage("Parola este necesara");
    }
}