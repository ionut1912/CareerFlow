using CareerFlow.Core.Domain.Assemblers;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Models.Assembly;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Domain.Test.AssemBlers;

public class CourseAssemblerTests
{
    private static SubchapterAssemblyModel CreateSub(string title = "Sub", string summary = "Sum", string html = "<p>T</p>")
        => new(title, summary, html, []);
 
    private static ChapterAssemblyModel CreateChapterModel(int day = 1, string title = "Chapter", string core = "Core")
        => new(day, title, core, [CreateSub()], []);
 
    private static (List<ChapterAssemblyModel> Models, Course Course) BuildMatchedData(int count = 1)
    {
        var models = Enumerable.Range(1, count).Select(i =>
            new ChapterAssemblyModel(i, $"Chapter {i}", $"Core {i}",
                [new SubchapterAssemblyModel($"Sub {i}", "Summary", "<p>x</p>", [])], []))
            .ToList();
 
        var chapters = models.Select(m =>
            Chapter.Create(m.Day, m.Title, m.CoreConcept,
                m.Subchapters.Select(s => SubChapter.Create(s.Title, s.ContentSummary, s.TheoryHtml)).ToList()))
            .ToList();
 
        return (models, Course.Create("Topic", chapters));
    }
 
    [Fact]
    public void BuildChapters_SingleModel_ReturnsOneChapter()
    {
        var result = CourseAssembler.BuildChapters([CreateChapterModel()]);
 
        result.Count.ShouldBe(1);
    }
 
    [Fact]
    public void BuildChapters_MultipleModels_ReturnsCorrectCount()
    {
        var data = new List<ChapterAssemblyModel> { CreateChapterModel(1), CreateChapterModel(2), CreateChapterModel(3) };
 
        var result = CourseAssembler.BuildChapters(data);
 
        result.Count.ShouldBe(3);
    }
 
    [Fact]
    public void BuildChapters_ValidModel_MapsAllFields()
    {
        var data = new List<ChapterAssemblyModel> { new(5, "My Chapter", "My Concept", [CreateSub("S1", "Sum1", "<p>H</p>")], []) };
 
        var result = CourseAssembler.BuildChapters(data);
 
        result[0].Day.ShouldBe(5);
        result[0].Title.ShouldBe("My Chapter");
        result[0].CoreConcept.ShouldBe("My Concept");
        result[0].SubChapters.Count.ShouldBe(1);
    }
 
    [Fact]
    public void BuildChapters_MultipleSubChapters_BuildsAll()
    {
        var data = new List<ChapterAssemblyModel>
        {
            new(1, "Title", "Core", [CreateSub("A", "S1", "<p>1</p>"), CreateSub("B", "S2", "<p>2</p>")], [])
        };
 
        var result = CourseAssembler.BuildChapters(data);
 
        result[0].SubChapters.Count.ShouldBe(2);
    }
 
    [Fact]
    public void BuildChapters_TitleExceeds200Chars_TruncatesToMaxLength()
    {
        var longTitle = new string('A', 250);
        var data = new List<ChapterAssemblyModel> { new(1, longTitle, "Core", [CreateSub()], []) };
 
        var result = CourseAssembler.BuildChapters(data);
 
        result[0].Title.Length.ShouldBeLessThanOrEqualTo(200);
    }
 
    [Fact]
    public void BuildChapters_TitleExactly200Chars_NotTruncated()
    {
        var title = new string('X', 200);
        var data = new List<ChapterAssemblyModel> { new(1, title, "Core", [CreateSub()], []) };
 
        var result = CourseAssembler.BuildChapters(data);
 
        result[0].Title.Length.ShouldBe(200);
    }
 
    [Fact]
    public void BuildChapters_LongTitleWithSpaces_TruncatesAtWordBoundary()
    {
        var title = new string('A', 190) + " LongWord";
        var data = new List<ChapterAssemblyModel> { new(1, title, "Core", [CreateSub()], []) };
 
        var result = CourseAssembler.BuildChapters(data);
 
        result[0].Title.ShouldNotContain("LongWord");
        result[0].Title.Length.ShouldBeLessThanOrEqualTo(200);
    }
 
    [Fact]
    public void BuildChapters_NullTitle_ReturnsEmptyString()
    {
        var data = new List<ChapterAssemblyModel> { new(1, null!, "Core", [CreateSub()], []) };
 
        var result = CourseAssembler.BuildChapters(data);
 
        result[0].Title.ShouldBe(string.Empty);
    }
 
    [Fact]
    public void BuildChapters_EmptyList_ReturnsEmptyList()
    {
        var result = CourseAssembler.BuildChapters([]);
 
        result.ShouldBeEmpty();
    }
 
    [Fact]
    public void BuildQuizQuestions_NoQuizData_ReturnsEmptyList()
    {
        var (models, course) = BuildMatchedData(1);
 
        var result = CourseAssembler.BuildQuizQuestions(models, course);
 
        result.ShouldBeEmpty();
    }
 
    [Fact]
    public void BuildQuizQuestions_RecapQuiz_CreatesQuestionWithChapterId()
    {
        var (models, course) = BuildMatchedData(1);
        var modelsWithQuiz = new List<ChapterAssemblyModel>
        {
            new(models[0].Day, models[0].Title, models[0].CoreConcept, models[0].Subchapters,
            [new QuizItemAssemblyModel("Q?", [new QuizOptionAssemblyModel("A", true), new QuizOptionAssemblyModel("B", false)])])
        };
 
        var result = CourseAssembler.BuildQuizQuestions(modelsWithQuiz, course);
 
        result.Count.ShouldBe(1);
        result[0].ChapterId.ShouldNotBeNull();
        result[0].SubChapterId.ShouldBeNull();
    }
 
    [Fact]
    public void BuildQuizQuestions_SubchapterQuiz_CreatesQuestionWithSubChapterId()
    {
        var subWithQuiz = new SubchapterAssemblyModel("Sub", "Sum", "<p>T</p>",
        [new QuizItemAssemblyModel("Q?", [new QuizOptionAssemblyModel("Correct", true)])]);
 
        var chapterModel = new ChapterAssemblyModel(1, "Ch", "Core", [subWithQuiz], []);
        var chapter = Chapter.Create(1, "Ch", "Core", [SubChapter.Create("Sub", "Sum", "<p>T</p>")]);
        var course = Course.Create("Topic", [chapter]);
 
        var result = CourseAssembler.BuildQuizQuestions([chapterModel], course);
 
        result.Count.ShouldBe(1);
        result[0].SubChapterId.ShouldNotBeNull();
        result[0].ChapterId.ShouldBeNull();
    }
 
    [Fact]
    public void BuildQuizQuestions_CorrectAnswerIsMappedFromFirstCorrectOption()
    {
        var (models, course) = BuildMatchedData(1);
        var modelsWithQuiz = new List<ChapterAssemblyModel>
        {
            new(models[0].Day, models[0].Title, models[0].CoreConcept, models[0].Subchapters,
            [new QuizItemAssemblyModel("Q?",
            [
                new QuizOptionAssemblyModel("Wrong Option", false),
                new QuizOptionAssemblyModel("Correct Answer", true)
            ])])
        };
 
        var result = CourseAssembler.BuildQuizQuestions(modelsWithQuiz, course);
 
        result[0].CorrectAnswer.ShouldBe("Correct Answer");
    }
 
    [Fact]
    public void BuildQuizQuestions_MultipleChaptersWithQuiz_CreatesQuestionsForAll()
    {
        var (models, course) = BuildMatchedData(2);
        var modelsWithQuiz = models.Select(m =>
            new ChapterAssemblyModel(m.Day, m.Title, m.CoreConcept, m.Subchapters,
            [new QuizItemAssemblyModel("Q?", [new QuizOptionAssemblyModel("A", true)])])).ToList();
 
        var result = CourseAssembler.BuildQuizQuestions(modelsWithQuiz, course);
 
        result.Count.ShouldBe(2);
    }
}