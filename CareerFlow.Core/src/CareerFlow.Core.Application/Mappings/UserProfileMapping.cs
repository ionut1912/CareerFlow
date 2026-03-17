using CareerFlow.Core.Application.CQRS.UserProfiles.Commands;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Requests;
using CareerFlow.Core.Domain.Entities;

namespace CareerFlow.Core.Application.Mappings;

public static class UserProfileMapping
{
    public static UserProfileDto ToDto(this UserProfile profile)
    {
        return new UserProfileDto(profile.Id, profile.AccountId, profile.Domain, profile.CorrectAnswersForQuiz,
            profile.IncorrectAnswersForQuiz, profile.Experience, profile.LearningType.Value,
            profile.UserTypes.Select(x => x.Value).ToList());
    }

    public static List<UserProfileDto> ToDtos(this IEnumerable<UserProfile> profiles)
    {
        return profiles.Select(ToDto).ToList();
    }

    public static CreateUserProfileCommand ToCreateUserProfileCommand(this CreateUserProfileRequest profileRequest,
        Guid accountId)
    {
        return new CreateUserProfileCommand(accountId, profileRequest.LearningType, profileRequest.UserTypes,
            profileRequest.Domain);
    }

    public static UpdateUserProfileCommand ToUpdateUserProfileCommand(this UpdateUserProfileRequest profileRequest,
        Guid id)
    {
        return new UpdateUserProfileCommand(id, profileRequest.LearningType, profileRequest.UserTypes,
            profileRequest.Domain);
    }
}