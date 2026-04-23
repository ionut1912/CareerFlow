using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Domain.Entities;

namespace CareerFlow.Core.Application.Mappings;

public static class ChapterMapping
{
    private static ChapterDto ToDto(this Chapter chapter) =>
        new(chapter.Day, chapter.Title, chapter.CoreConcept, chapter.SubChapters.ToDto());

    public static List<ChapterDto> ToDto(this IEnumerable<Chapter> chapters) =>
        chapters.Select(c => c.ToDto()).ToList();
}
