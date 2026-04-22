using JetBrains.Annotations;

using Shared.Domain.Common;

namespace CareerFlow.Core.Domain.Entities;

public class Chapter : Entity
{
    private readonly List<SubChapter> _subChapters = [];

    [UsedImplicitly]
    private Chapter() //For EfCore
    {
    }

    private Chapter(int day, string title, string coreConcept, List<SubChapter> subChapters)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new InvalidDataException("Titlul capitolului este necesar");
        if (string.IsNullOrWhiteSpace(coreConcept))
            throw new InvalidDataException("Core concept este necesar");
        if (subChapters.Count==0)
            throw new InvalidDataException("Sub chapters este necesar");

        Day = day;
        Title = title;
        CoreConcept = coreConcept;
        _subChapters.AddRange(subChapters);
    }

    [UsedImplicitly]
    public Guid CourseId { get; private set; }
    [UsedImplicitly]
    public Course? Course { get; private set; }
    public int Day { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string CoreConcept { get; private set; } = string.Empty;
    public IReadOnlyCollection<SubChapter> SubChapters => _subChapters.AsReadOnly();

    public static Chapter Create(int day, string title, string coreConcept, List<SubChapter> subChapters) => new(day, title, coreConcept, subChapters);

    public void SetCourseId(Guid courseId) => CourseId = courseId;
}
