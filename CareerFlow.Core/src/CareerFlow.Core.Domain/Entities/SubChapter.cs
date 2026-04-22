using CareerFlow.Core.Domain.Exceptions;
using JetBrains.Annotations;
using Shared.Domain.Common;

namespace CareerFlow.Core.Domain.Entities;

public class SubChapter : Entity
{
    [UsedImplicitly]
    private SubChapter()//For EfCore
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

    public string Title { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public string TheoryHtml { get; private set; } = string.Empty;
    [UsedImplicitly]
    public Guid ChapterId { get; private set; }
    [UsedImplicitly]
    public Chapter? Chapter { get; private set; }

    public static SubChapter Create(string title, string summary, string theoryHtml) => new(title, summary, theoryHtml);
}
