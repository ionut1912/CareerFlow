using CareerFlow.Core.Application.CQRS.UserProfiles.Commands;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Requests.UserProfile;
using CareerFlow.Core.Domain.Entities;

namespace CareerFlow.Core.Application.Mappings;

public static class UserProfileMapping
{
    public static UserProfileDto ToDto(this UserProfile profile)
    {
        string email = profile.Account?.Email ?? string.Empty;
        string username = profile.Account?.Username ?? string.Empty;
        string name = profile.Account?.Name ?? string.Empty;

        List<CourseDto> courses = profile.Courses.ToDto();

        var userTypes = profile.UserTypes
            .Select(ut => ut.Value)
            .ToList();

        return new UserProfileDto(
            profile.Id,
            profile.AccountId,
            email,
            username,
            name,
            profile.Domain,
            profile.CorrectAnswersForQuiz,
            profile.IncorrectAnswersForQuiz,
            profile.Experience,
            profile.LearningType.Value,
            userTypes,
            courses);
    }

    public static List<UserProfileDto> ToDtos(this IEnumerable<UserProfile> profiles) =>
        profiles.Select(ToDto).ToList();

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
