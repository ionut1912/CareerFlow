using CareerFlow.Core.Domain.Models.AI.Dto;
using CareerFlow.Core.Domain.Models.Assembly;

namespace CareerFlow.Core.Infrastructure.Mappers;

public static class ExpandedChapterDataDtoExtensions
{
    public static List<ChapterAssemblyModel> ToAssemblyModels(this List<ExpandedChapterDataDto> data)
    {
        return data.Select(ch => new ChapterAssemblyModel(
            ch.Day,
            ch.Title,
            ch.CoreConcept,
            ch.Details.Subchapters.Select(s => new SubchapterAssemblyModel(
                s.Title,
                s.ContentSummary,
                s.TheoryHtml,
                s.Quiz.Select(ToQuizItem).ToList()
            )).ToList(),
            ch.Details.RecapQuiz.Select(ToQuizItem).ToList()
        )).ToList();
    }

    private static QuizItemAssemblyModel ToQuizItem(QuestionDto q)
    {
        return new QuizItemAssemblyModel(q.Question,
            q.Options.Select(o => new QuizOptionAssemblyModel(o.Label, o.IsCorrect)).ToList());
    }
}
