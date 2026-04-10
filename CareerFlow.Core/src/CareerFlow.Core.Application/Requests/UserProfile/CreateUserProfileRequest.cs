namespace CareerFlow.Core.Application.Requests.UserProfile;

public sealed record CreateUserProfileRequest(string LearningType, List<string> UserTypes, string? Domain);