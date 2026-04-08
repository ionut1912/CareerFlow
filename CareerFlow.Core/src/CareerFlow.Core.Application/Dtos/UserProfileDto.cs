namespace CareerFlow.Core.Application.Dtos;

public record UserProfileDto(
    Guid Id,
    Guid AccountId,
    string Email,
    string Username,
    string Name,
    string Domain,
    int CorrectAnswersForQuiz,
    int IncorrectAnswersForQuiz,
    int Experience,
    string LearningType,
    List<string> UserTypes);