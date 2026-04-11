namespace CareerFlow.Core.Application.Requests.UserProfile;

public record UpdateUserProfileRequest(string LearningType, List<string> UserTypes, string Domain);