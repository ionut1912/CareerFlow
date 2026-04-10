using System.Collections.Concurrent;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Models.AI.Dto;
using CareerFlow.Core.Domain.Models.AI.Requests;
using CareerFlow.Core.Domain.Models.AI.Responses;
using CareerFlow.Core.Domain.ValueObjects;
using CareerFlow.Core.Infrastructure.Mappers;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Infrastructure.HangfireJobs;

[AutomaticRetry(Attempts = 3, DelaysInSeconds = [60, 120, 300])]
public sealed class ProcessCourseJob
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
        var job = await _courseJobRepository.GetByIdAsync(jobId, ct, j => j.Upload!);

        if (job is null)
        {
            _logger.LogWarning("Job {JobId} not found", jobId);
            return;
        }

        await UpdateJobStatusAsync(job, JobStatus.Processing, ct);

        try
        {
            var documentResponse = await GetOrCacheDocumentAnalysisAsync(job.Upload!, ct);
            var expandedChapters = await GetOrCacheExpandedChaptersAsync(job.Upload!, documentResponse, ct);

            var courseId = await _coursePersistenceService.PersistAsync(
                userId, job.Upload!.Title, expandedChapters.ToAssemblyModels(), ct);

            job.SetCourseId(courseId);
            await UpdateJobStatusAsync(job, JobStatus.Done, ct);

            _logger.LogInformation("Job {JobId} done. Course {CourseId}", jobId, courseId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job {JobId} failed", jobId);
            job.SetErrorMessage(ex.Message);
            await UpdateJobStatusAsync(job, JobStatus.Failed, ct);
            throw;
        }
    }

    private async Task<DocumentProcessingResponse> GetOrCacheDocumentAnalysisAsync(
        CourseUpload upload, CancellationToken ct)
    {
        var cacheKey = $"course:analyze:{upload.FileName}";
        var cached = await _cache.GetAsync<DocumentProcessingResponse>(cacheKey, ct);

        if (cached is not null)
        {
            _logger.LogInformation("Cache hit for analysis of {FileName}", upload.FileName);
            return cached;
        }

        var fileBytes = await DownloadFileBytesAsync(upload.FileKey, ct);
        var formFile = CreateFormFile(fileBytes, upload.FileName);
        var response = await _analyzer.AnalyzeDocumentAsync(formFile, ct);

        await _cache.SetAsync(cacheKey, response, TimeSpan.FromHours(2), ct);
        return response;
    }

    private async Task<List<ExpandedChapterDataDto>> GetOrCacheExpandedChaptersAsync(
        CourseUpload upload, DocumentProcessingResponse documentResponse, CancellationToken ct)
    {
        var cacheKey = $"course:chapters:{upload.FileName}";
        var cached = await _cache.GetAsync<List<ExpandedChapterDataDto>>(cacheKey, ct);

        if (cached is not null)
        {
            _logger.LogInformation("Cache hit for chapters of {FileName}", upload.FileName);
            return cached;
        }

        var concurrentChapters = new ConcurrentBag<ExpandedChapterDataDto>();
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 6, CancellationToken = ct };

        await Parallel.ForEachAsync(documentResponse.Skeleton.Chapters, parallelOptions, async (chapterSkeleton, token) =>
        {
            var request = new DocumentChapterRequest(
                chapterSkeleton.Title,
                chapterSkeleton.CoreConcept,
                documentResponse.DocumentId);

            var detail = await _analyzer.ExpandAnalyzedDocument(request, token);

            concurrentChapters.Add(new ExpandedChapterDataDto(
                chapterSkeleton.Day,
                chapterSkeleton.Title,
                chapterSkeleton.CoreConcept,
                detail));
        });

        var result = concurrentChapters.OrderBy(c => c.Day).ToList();
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromHours(2), ct);
        return result;
    }

    private async Task<byte[]> DownloadFileBytesAsync(string fileKey, CancellationToken ct)
    {
        await using var stream = await _storage.DownloadAsync(fileKey, ct);
        var ms = new MemoryStream();
        await stream.CopyToAsync(ms, ct);
        return ms.ToArray();
    }

    private static IFormFile CreateFormFile(byte[] bytes, string fileName)
    {
        var ms = new MemoryStream(bytes);
        return new FormFile(ms, 0, ms.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/octet-stream"
        };
    }

    private async Task UpdateJobStatusAsync(CourseJob job, JobStatus status, CancellationToken ct)
    {
        job.Update(status);
        await _uow.SaveChangesAsync(ct);
    }
}