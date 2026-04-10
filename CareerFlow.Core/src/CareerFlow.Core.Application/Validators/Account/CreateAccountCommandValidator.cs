using CareerFlow.Core.Application.CQRS.Accounts.Commands;
using FluentValidation;

namespace CareerFlow.Core.Application.Validators.Account;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(a => a.Password).NotEmpty().WithMessage("Parola este necesara");
        RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage("Parola de confirmare este necesara");
        RuleFor(a => a.Username).NotEmpty().WithMessage("Numele de utilizator este necesar");
        RuleFor(a => a.Email).NotEmpty().WithMessage("Email-ul este necesar")
            .EmailAddress().WithMessage("Email-ul are format invalid");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Numele este necesar");
    }
}