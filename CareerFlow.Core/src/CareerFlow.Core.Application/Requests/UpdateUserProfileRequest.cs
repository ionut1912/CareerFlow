namespace CareerFlow.Core.Application.Requests;

public record UpdateUserProfileRequest(string LearningType, List<string> UserTypes, string Domain);