using CareerFlow.Core.Application.Mediatr.Accounts.Commands;
using FluentValidation;

namespace CareerFlow.Core.Application.Validators.Account;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(a => a.Password).NotEmpty().WithMessage("Password is required");
        RuleFor(a => a.Username).NotEmpty().WithMessage("Username is required");
        RuleFor(a => a.Email).NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email has invalid format");
    }
}