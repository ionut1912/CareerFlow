namespace CareerFlow.Core.Application.CQRS.UserProfiles.Commands;

public record UpdateUserProfileCommand(Guid Id, string LearningType, List<string> UserTypes, string Domain);
