using CareerFlow.Core.Domain.Exceptions;
using Shared.Domain.Common;

namespace CareerFlow.Core.Domain.Entities;

public class SubChapter : Entity
{
    private SubChapter()
    {
    }

    private SubChapter(string title, string summary, string theoryHtml)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new InvalidFieldException("Titlul subcapitolului este necesar");

        if (string.IsNullOrWhiteSpace(summary))
            throw new InvalidFieldException("Rezumatul subcapitolului este necesar");


        if (string.IsNullOrWhiteSpace(theoryHtml))
            throw new InvalidFieldException("Continutul subcapitolului este necesar");

        Title = title;
        Summary = summary;
        TheoryHtml = theoryHtml;
    }

    public string Title { get; private set; }
    public string Summary { get; private set; }
    public string TheoryHtml { get; private set; }
    public Guid ChapterId { get; private set; }
    public Chapter Chapter { get; private set; }

    public static SubChapter Create(string title, string summary, string theoryHtml)
    {
        return new SubChapter(title, summary, theoryHtml);
    }
}