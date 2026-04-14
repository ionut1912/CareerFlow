using Shared.Domain.Common;

namespace CareerFlow.Core.Domain.Entities;

public sealed class QuizQuestion : Entity
{
    private QuizQuestion()
    {
    }

    private QuizQuestion(string question, List<string> options,
        string correctAnswer, Guid? chapterId, Guid? subChapterId)
    {
        Question = question;
        Options = options;
        CorrectAnswer = correctAnswer;
        ChapterId = chapterId;
        SubChapterId = subChapterId;
    }

    public string Question { get; private set; } = string.Empty;
    public List<string> Options { get; private set; } = [];
    public string CorrectAnswer { get; private set; } = string.Empty;
    public Guid? ChapterId { get; private set; }
    public Guid? SubChapterId { get; private set; }
    public Chapter? Chapter { get; private set; }
    public SubChapter? SubChapter { get; private set; }

    public static QuizQuestion Create(string question, List<string> options, string correctAnswer, Guid? chapterId,
        Guid? subChapterId)
    {
        return new QuizQuestion(question, options, correctAnswer, chapterId, subChapterId);
    }
}