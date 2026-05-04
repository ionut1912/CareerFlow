using CareerFlow.Core.Domain.Models.AI.Dto;
using CareerFlow.Core.Domain.Models.AI.Responses;
using CareerFlow.Core.Domain.Models.Assembly;

namespace CareerFlow.Core.Infrastructure.Mappers;

public static class ChapterExpandResponseExtensions
{
    public static List<ChapterAssemblyModel> ToAssemblyModels(this List<ChapterExpandResponse> responses)
    {
        return responses.Select(r => new ChapterAssemblyModel(
            r.Chapter.Day,
            r.Chapter.Title,
            r.Chapter.CoreConcept,
            r.Expanded.Subchapters
                .Zip(r.SubchapterContents, (sub, content) => new SubchapterAssemblyModel(
                    sub.Title,
                    sub.ContentSummary,
                    content.TheoryHtml,
                    content.Quiz.Select(ToQuizItem).ToList()))
                .ToList(),
            r.FinalQuiz.Select(ToQuizItem).ToList()
        )).ToList();
    }

    private static QuizItemAssemblyModel ToQuizItem(QuestionDto q)
    {
        return new QuizItemAssemblyModel(q.Question,
            q.Options.Select(o => new QuizOptionAssemblyModel(o.Label, o.IsCorrect)).ToList());
    }
}
