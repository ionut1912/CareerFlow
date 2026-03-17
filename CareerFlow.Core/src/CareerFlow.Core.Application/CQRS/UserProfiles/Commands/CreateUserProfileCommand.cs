namespace CareerFlow.Core.Application.CQRS.UserProfiles.Commands;

public record CreateUserProfileCommand(Guid AccountId, string LearningType, List<string> UserTypes, string? Domain)
{
}