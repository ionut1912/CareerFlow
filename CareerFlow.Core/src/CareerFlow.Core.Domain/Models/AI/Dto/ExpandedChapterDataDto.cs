using CareerFlow.Core.Domain.Models.AI.Responses;

namespace CareerFlow.Core.Domain.Models.AI.Dto;

public sealed record ExpandedChapterDataDto(
    int Day,
    string Title,
    string CoreConcept,
    ChapterDetailResponse Details
);