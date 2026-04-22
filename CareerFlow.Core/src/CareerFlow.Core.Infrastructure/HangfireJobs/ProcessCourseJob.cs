using System.Collections.Concurrent;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Constants;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Models.AI.Dto;
using CareerFlow.Core.Domain.Models.AI.Requests;
using CareerFlow.Core.Domain.Models.AI.Responses;
using CareerFlow.Core.Domain.Models.Course.Dto;
using CareerFlow.Core.Domain.ValueObjects;
using CareerFlow.Core.Infrastructure.Mappers;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Infrastructure.HangfireJobs;

[AutomaticRetry(Attempts = 3, DelaysInSeconds = [60, 120, 300])]
public sealed partial class ProcessCourseJob
{
    private readonly IDocumentAnalyzerService _analyzer;
    private readonly ICacheService _cache;
    private readonly ICourseJobRepository _courseJobRepository;
    private readonly ICoursePersistenceService _coursePersistenceService;
    private readonly ILogger<ProcessCourseJob> _logger;
    private readonly IStorageService _storage;
    private readonly IUnitOfWork _uow;

    public ProcessCourseJob(
        IUnitOfWork uow,
        IStorageService storage,
        IDocumentAnalyzerService analyzer,
        ILogger<ProcessCourseJob> logger,
        ICourseJobRepository courseJobRepository,
        ICoursePersistenceService coursePersistenceService,
        ICacheService cache)
    {
        ArgumentNullException.ThrowIfNull(uow);
        ArgumentNullException.ThrowIfNull(storage);
        ArgumentNullException.ThrowIfNull(analyzer);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(courseJobRepository);
        ArgumentNullException.ThrowIfNull(coursePersistenceService);
        ArgumentNullException.ThrowIfNull(cache);

        _uow = uow;
        _storage = storage;
        _analyzer = analyzer;
        _logger = logger;
        _courseJobRepository = courseJobRepository;
        _coursePersistenceService = coursePersistenceService;
        _cache = cache;
    }

    public async Task ExecuteAsync(Guid jobId, Guid userId, CancellationToken ct)
    {
        CourseJob? job = await _courseJobRepository.GetByIdAsync(jobId, ct, j => j.Upload!);

        if (job is null)
        {
            LogJobNotFound(jobId);
            return;
        }

        await UpdateJobStatusAsync(job, JobStatus.Processing, ct);

        try
        {
            DocumentProcessingResponse documentResponse = await GetOrCacheDocumentAnalysisAsync(job.Upload!, ct);
            List<ExpandedChapterDataDto> expandedChapters = await GetOrCacheExpandedChaptersAsync(job.Upload!, documentResponse, ct);

            Guid courseId = await _coursePersistenceService.PersistAsync(
                userId, job.Upload!.Title, expandedChapters.ToAssemblyModels(), ct);

            job.SetCourseId(courseId);
            await UpdateJobStatusAsync(job, JobStatus.Done, ct);

            LogJobDone(jobId, courseId);
        }
        catch (Exception ex)
        {
            LogJobFailed(ex, jobId);
            job.SetErrorMessage(ex.Message);
            await UpdateJobStatusAsync(job, JobStatus.Failed, ct);
            throw;
        }
    }

    private async Task<DocumentProcessingResponse> GetOrCacheDocumentAnalysisAsync(
        CourseUpload upload, CancellationToken ct)
    {
        string cacheKey = CacheKeyConstants.CacheKeyDocAnalyze(upload.FileName);
        DocumentProcessingResponse? cached = await _cache.GetAsync<DocumentProcessingResponse>(cacheKey);

        if (cached is not null)
        {
            LogCacheHitAnalysis(upload.FileName);
            return cached;
        }

        UploadFileDto uploadFile = await DownloadAsUploadFileModelAsync(upload, ct);
        DocumentProcessingResponse response = await _analyzer.AnalyzeDocumentAsync(uploadFile, ct);

        await _cache.SetAsync(cacheKey, response, TimeSpan.FromHours(2));
        return response;
    }

    private async Task<List<ExpandedChapterDataDto>> GetOrCacheExpandedChaptersAsync(
        CourseUpload upload, DocumentProcessingResponse documentResponse, CancellationToken ct)
    {
        string cacheKey = CacheKeyConstants.CacheKeyChapter(upload.FileName);
        List<ExpandedChapterDataDto>? cached = await _cache.GetAsync<List<ExpandedChapterDataDto>>(cacheKey);

        if (cached is not null)
        {
            LogCacheHitChapters(upload.FileName);
            return cached;
        }

        var concurrentChapters = new ConcurrentBag<ExpandedChapterDataDto>();
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 6, CancellationToken = ct };

        await Parallel.ForEachAsync(documentResponse.Skeleton.Chapters, parallelOptions,
            async (chapterSkeleton, token) =>
            {
                var request = new DocumentChapterRequest(
                    chapterSkeleton.Title,
                    chapterSkeleton.CoreConcept,
                    documentResponse.DocumentId);

                ChapterDetailResponse detail = await _analyzer.ExpandAnalyzedDocument(request, token);

                concurrentChapters.Add(new ExpandedChapterDataDto(
                    chapterSkeleton.Day,
                    chapterSkeleton.Title,
                    chapterSkeleton.CoreConcept,
                    detail));
            });

        var result = concurrentChapters.OrderBy(c => c.Day).ToList();
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromHours(2));
        return result;
    }

    private async Task<UploadFileDto> DownloadAsUploadFileModelAsync(CourseUpload upload, CancellationToken ct)
    {
        Stream stream = await _storage.DownloadAsync(upload.FileKey, ct);
        var ms = new MemoryStream();
        await stream.CopyToAsync(ms, ct);
        ms.Position = 0;
        return new UploadFileDto(upload.FileName, "application/octet-stream", ms);
    }

    private async Task UpdateJobStatusAsync(CourseJob job, JobStatus status, CancellationToken ct)
    {
        job.Update(status);
        await _uow.SaveChangesAsync(ct);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Job {JobId} not found")]
    private partial void LogJobNotFound(Guid jobId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Job {JobId} done. Course {CourseId}")]
    private partial void LogJobDone(Guid jobId, Guid courseId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Job {JobId} failed")]
    private partial void LogJobFailed(Exception ex, Guid jobId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Cache hit for analysis of {FileName}")]
    private partial void LogCacheHitAnalysis(string fileName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Cache hit for chapters of {FileName}")]
    private partial void LogCacheHitChapters(string fileName);
}
