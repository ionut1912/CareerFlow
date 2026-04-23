using JetBrains.Annotations;
using Shared.Domain.Common;

namespace CareerFlow.Core.Domain.Entities;

public class Course : Entity
{
    private readonly List<Chapter> _chapters = [];

    [UsedImplicitly]
    private readonly List<UserProfile> _userProfiles = [];

    [UsedImplicitly]
    private Course()
    {
    }

    private Course(string topic, List<Chapter> chapters)
    {
        if (string.IsNullOrWhiteSpace(topic))
            throw new InvalidDataException("Topic este necesar");
        if (chapters.Count == 0)
            throw new InvalidDataException("Chapters este necesar");

        Topic = topic;
        _chapters.AddRange(chapters);
    }

    public string Topic { get; private set; } = string.Empty;
    public IReadOnlyCollection<Chapter> Chapters => _chapters.AsReadOnly();
    public IReadOnlyCollection<UserProfile> UserProfiles => _userProfiles.AsReadOnly();

    public static Course Create(string topic, List<Chapter> chapters) => new(topic, chapters);
}
