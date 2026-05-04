using CareerFlow.Core.Application.CQRS.UserProfiles.Commands;

using FluentValidation;

namespace CareerFlow.Core.Application.Validators.UserProfiles;

public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    private static readonly IReadOnlyCollection<string> _validLearningTypes =
        ["Visual", "Auditory", "ReadWrite", "Combined"];

    private static readonly IReadOnlyCollection<string> _validUserTypes =
        ["Student", "JobSearcher", "HobbyLearner"];

    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.LearningType)
            .Must(x => _validLearningTypes.Contains(x))
            .WithMessage($"Tipul de invatare trebuie sa fie unul dinte: {string.Join(", ", _validLearningTypes)}.");

        RuleFor(x => x.UserTypes)
            .Must(x => x.Count <= _validUserTypes.Count)
            .WithMessage("Prea multe tipuri.")
            .Must(x => x.All(_validUserTypes.Contains))
            .WithMessage($"Fiecare tip de user trebuie sa fie unul dintre: {string.Join(", ", _validUserTypes)}.")
            .Must(x => x.Distinct().Count() == x.Count)
            .WithMessage("Tipul user nu trebuie sa contina duplicate");

        RuleFor(x => x.Domain)
            .MaximumLength(100).WithMessage("Domeniul nu trebuie sa aiba mai mult de 100 caractere.");
    }
}
