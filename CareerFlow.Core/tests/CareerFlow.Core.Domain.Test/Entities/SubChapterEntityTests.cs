using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Domain.Test;

public class SubChapterEntityTests
{
    [Fact]
    public void Create_ValidParameters_ReturnsSubChapter()
    {
        var sub = SubChapter.Create("Title", "Summary", "<p>Theory</p>");
 
        sub.ShouldNotBeNull();
        sub.Title.ShouldBe("Title");
        sub.Summary.ShouldBe("Summary");
        sub.TheoryHtml.ShouldBe("<p>Theory</p>");
    }
 
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_InvalidTitle_ThrowsInvalidFieldException(string? title)
    {
        Should.Throw<InvalidFieldException>(() =>
            SubChapter.Create(title!, "Summary", "<p>x</p>"));
    }
 
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_InvalidSummary_ThrowsInvalidFieldException(string? summary)
    {
        Should.Throw<InvalidFieldException>(() =>
            SubChapter.Create("Title", summary!, "<p>x</p>"));
    }
 
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_InvalidTheoryHtml_ThrowsInvalidFieldException(string? html)
    {
        Should.Throw<InvalidFieldException>(() =>
            SubChapter.Create("Title", "Summary", html!));
    }
}