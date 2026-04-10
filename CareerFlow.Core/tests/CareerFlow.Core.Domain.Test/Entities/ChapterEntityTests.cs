using CareerFlow.Core.Domain.Entities;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Domain.Test;

public class ChapterEntityTests
{
    private static SubChapter Sub() => SubChapter.Create("Sub Title", "Summary", "<p>T</p>");
 
    [Fact]
    public void Create_ValidParameters_ReturnsChapter()
    {
        var chapter = Chapter.Create(1, "Title", "Core", [Sub()]);
 
        chapter.ShouldNotBeNull();
        chapter.Day.ShouldBe(1);
        chapter.Title.ShouldBe("Title");
        chapter.CoreConcept.ShouldBe("Core");
        chapter.SubChapters.ShouldHaveSingleItem();
    }
 
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_InvalidTitle_ThrowsInvalidDataException(string? title)
    {
        Should.Throw<InvalidDataException>(() =>
            Chapter.Create(1, title!, "Core", [Sub()]));
    }
 
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_InvalidCoreConcept_ThrowsInvalidDataException(string? core)
    {
        Should.Throw<InvalidDataException>(() =>
            Chapter.Create(1, "Title", core!, [Sub()]));
    }
 
    [Fact]
    public void Create_EmptySubChapters_ThrowsInvalidDataException()
    {
        Should.Throw<InvalidDataException>(() =>
            Chapter.Create(1, "Title", "Core", []));
    }
 
    [Fact]
    public void Create_MultipleSubChapters_AllAdded()
    {
        var chapter = Chapter.Create(1, "Title", "Core", [Sub(), Sub(), Sub()]);
 
        chapter.SubChapters.Count.ShouldBe(3);
    }
 
    [Fact]
    public void SetCourseId_ValidId_SetsCourseId()
    {
        var chapter = Chapter.Create(1, "Title", "Core", [Sub()]);
        var courseId = Guid.NewGuid();
 
        chapter.SetCourseId(courseId);
 
        chapter.CourseId.ShouldBe(courseId);
    }
}