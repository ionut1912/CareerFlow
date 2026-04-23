using JetBrains.Annotations;

using Shared.Domain.Common;

namespace CareerFlow.Core.Domain.Entities;

public sealed class QuizQuestion : Entity
{
    [UsedImplicitly]
    private QuizQuestion() //For EfCore
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

    [UsedImplicitly] public Guid? SubChapterId { get; private set; }

    [UsedImplicitly] public Chapter? Chapter { get; private set; }

    [UsedImplicitly] public SubChapter? SubChapter { get; private set; }

    public static QuizQuestion Create(string question, List<string> options, string correctAnswer, Guid? chapterId,
        Guid? subChapterId) =>
        new(question, options, correctAnswer, chapterId, subChapterId);
}
