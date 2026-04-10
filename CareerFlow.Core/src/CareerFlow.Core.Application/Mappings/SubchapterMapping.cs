using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Domain.Entities;

namespace CareerFlow.Core.Application.Mappings;

public  static class SubchapterMapping
{
    private static SubChapterDto ToDto(this SubChapter subChapter)
    {
        return new SubChapterDto(subChapter.Title,subChapter.Summary,subChapter.TheoryHtml);
    }

    public static List<SubChapterDto> ToDto(this IEnumerable<SubChapter> subChapters)
    {
        return subChapters.Select(s => s.ToDto()).ToList();
    }
}