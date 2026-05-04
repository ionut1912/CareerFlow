namespace CareerFlow.Core.Application.Requests.UserProfile;

public sealed record UpdateUserProfileRequest(string LearningType, List<string> UserTypes, string Domain);
