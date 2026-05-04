using CareerFlow.Core.Application.CQRS.UserProfiles.Commands;

using FluentValidation;

namespace CareerFlow.Core.Application.Validators.UserProfiles;

public class CreateUserProfileCommandValidator : AbstractValidator<CreateUserProfileCommand>
{
    public static readonly IReadOnlyCollection<string> ValidLearningTypes =
        ["Visual", "Auditory", "ReadWrite", "Combined"];

    public static readonly IReadOnlyCollection<string> ValidUserTypes =
        ["Student", "JobSearcher", "HobbyLearner"];

    public CreateUserProfileCommandValidator()
    {
        RuleFor(x => x.LearningType)
            .NotEmpty().WithMessage("Tipul de invatare este necesar.")
            .Must(x => ValidLearningTypes.Contains(x))
            .WithMessage($"Tipul de invatare trebuie sa fie unul dinte: {string.Join(", ", ValidLearningTypes)}.");

        RuleFor(x => x.UserTypes)
            .NotEmpty().WithMessage("Cel putin un tip pentru user este necesar.")
            .Must(x => x == null || x.Count <= ValidUserTypes.Count)
            .WithMessage("Prea multe tipuri.")
            .Must(x => x == null || x.All(ValidUserTypes.Contains))
            .WithMessage($"Fiecare tip de user trebuie sa fie unul dintre: {string.Join(", ", ValidUserTypes)}.")
            .Must(x => x == null || x.Distinct().Count() == x.Count)
            .WithMessage("Tipul user nu trebuie sa contina duplicate");

        RuleFor(x => x.Domain)
            .MaximumLength(100).WithMessage("Domeniul nu trebuie sa aiba mai mult de 100 caractere.")
            .When(x => x.Domain is not null);
    }
}
