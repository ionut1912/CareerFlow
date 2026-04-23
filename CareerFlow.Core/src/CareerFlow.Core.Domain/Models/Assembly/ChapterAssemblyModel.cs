namespace CareerFlow.Core.Domain.Models.Assembly;

public sealed record ChapterAssemblyModel(
    int Day,
    string Title,
    string CoreConcept,
    List<SubchapterAssemblyModel> Subchapters,
    List<QuizItemAssemblyModel> RecapQuiz);
