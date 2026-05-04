namespace CareerFlow.Core.Application.Dtos;

public sealed record ChapterDto(int Day, string Title, string CoreConcept, List<SubChapterDto> SubChapters);
