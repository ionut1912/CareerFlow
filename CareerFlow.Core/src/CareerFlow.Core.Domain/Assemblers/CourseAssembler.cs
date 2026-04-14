using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Models.Assembly;

namespace CareerFlow.Core.Domain.Assemblers;

public static class CourseAssembler
{
    public static List<Chapter> BuildChapters(List<ChapterAssemblyModel> data) =>
        data.Select(ch =>
        {
            var subChapters = ch.Subchapters
                .Select(s => SubChapter.Create(Truncate(s.Title), Truncate(s.ContentSummary), s.TheoryHtml))
                .ToList();

            return Chapter.Create(ch.Day, Truncate(ch.Title), Truncate(ch.CoreConcept), subChapters);
        }).ToList();

    public static List<QuizQuestion> BuildQuizQuestions(List<ChapterAssemblyModel> data, Course course)
    {
        var questions = new List<QuizQuestion>();

        foreach (var (chData, chapter) in data.Zip(course.Chapters))
        {
            if (chData.RecapQuiz is { Count: > 0 })
            {
                questions.AddRange(chData.RecapQuiz.Select(q =>
                    QuizQuestion.Create(
                        Truncate(q.Question, 500),
                        q.Options.Select(o => Truncate(o.Label)).ToList(),
                        Truncate(q.Options.FirstOrDefault(o => o.IsCorrect)?.Label),
                        chapter.Id,
                        null)));
            }

            foreach (var (sub, subChapter) in chData.Subchapters.Zip(chapter.SubChapters))
            {
                if (sub.Quiz is { Count: > 0 })
                {
                    questions.AddRange(sub.Quiz.Select(q =>
                        QuizQuestion.Create(
                            Truncate(q.Question, 500),
                            q.Options.Select(o => Truncate(o.Label)).ToList(),
                            Truncate(q.Options.FirstOrDefault(o => o.IsCorrect)?.Label),
                            null,
                            subChapter.Id)));
                }
            }
        }

        return questions;
    }

    private static string Truncate(string? value, int max = 200)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= max)
            return value ?? string.Empty;

        var truncated = value[..max];
        var lastSpace = truncated.LastIndexOf(' ');
        return lastSpace > 0 ? truncated[..lastSpace] : truncated;
    }
}