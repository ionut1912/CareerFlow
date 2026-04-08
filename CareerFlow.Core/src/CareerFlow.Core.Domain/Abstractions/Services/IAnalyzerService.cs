using CareerFlow.Core.Domain.Models.AI;

namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface IAnalyzerService
{
    Task<LearningPlanSkeletonResponse> GetSkeletonAsync(
        string topic, int numberOfDays, CancellationToken ct = default);

    Task<ExpandedDayResponse> ExpandChapterAsync(
        string topic, ChapterSkeletonDto chapter, CancellationToken ct = default);

    Task<SubchapterContentResponse> GetSubchapterContentAsync(
        string topic, string chapterTitle,
        SubchapterSkeletonDto subchapter, CancellationToken ct = default);

    Task<ChapterQuizResponse> GetChapterQuizAsync(
        string topic, string chapterTitle,
        List<SubchapterSkeletonDto> subchapters, CancellationToken ct = default);
}