namespace CareerFlow.Core.Application.Requests.UserProfile;

public record CreateUserProfileRequest(string LearningType, List<string> UserTypes, string? Domain);
