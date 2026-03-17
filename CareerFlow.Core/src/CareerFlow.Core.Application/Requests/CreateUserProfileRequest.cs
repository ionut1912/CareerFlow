namespace CareerFlow.Core.Application.Requests;

public record CreateUserProfileRequest(string LearningType, List<string> UserTypes, string? Domain);