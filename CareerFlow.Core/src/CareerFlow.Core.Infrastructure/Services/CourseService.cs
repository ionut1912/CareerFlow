using System.Collections.Concurrent;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Constants;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.Models.AI.Requests;
using CareerFlow.Core.Domain.Models.Course.Dto;
using CareerFlow.Core.Domain.Models.Course.Response;
using CareerFlow.Core.Domain.Models.Responses;
using CareerFlow.Core.Infrastructure.HangfireJobs;
using CareerFlow.Core.Infrastructure.Mappers;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Infrastructure.Services;

public sealed class CourseService : ICourseService
{
    private readonly IAnalyzerService _analyzer;
    private readonly ICacheService _cacheService;
    private readonly IChapterRepository _chapterRepository;
    private readonly ICourseJobRepository _courseJobRepository;
    private readonly ICoursePersistenceService _coursePersistenceService;
    private readonly ICourseUploadsRepository _courseUploadsRepository;
    private readonly IBackgroundJobClient _jobClient;
    private readonly ILogger<CourseService> _logger;
    private readonly IStorageService _storage;
    private readonly IUnitOfWork _uow;
    private readonly IUserProfileRepository _userProfileRepository;

    public CourseService(
        IStorageService storage,
        IBackgroundJobClient jobClient,
        ILogger<CourseService> logger,
        IUnitOfWork uow,
        ICacheService cacheService,
        ICourseUploadsRepository courseUploadsRepository,
        ICourseJobRepository courseJobRepository,
        IUserProfileRepository userProfileRepository,
        IChapterRepository chapterRepository,
        IAnalyzerService analyzer,
        ICoursePersistenceService coursePersistenceService)
    {
        ArgumentNullException.ThrowIfNull(storage);
        ArgumentNullException.ThrowIfNull(jobClient);
        ArgumentNullException.ThrowIfNull(uow);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(courseUploadsRepository);
        ArgumentNullException.ThrowIfNull(courseJobRepository);
        ArgumentNullException.ThrowIfNull(userProfileRepository);
        ArgumentNullException.ThrowIfNull(chapterRepository);
        ArgumentNullException.ThrowIfNull(analyzer);
        ArgumentNullException.ThrowIfNull(cacheService);
        ArgumentNullException.ThrowIfNull(coursePersistenceService);

        _storage = storage;
        _jobClient = jobClient;
        _uow = uow;
        _courseUploadsRepository = courseUploadsRepository;
        _courseJobRepository = courseJobRepository;
        _userProfileRepository = userProfileRepository;
        _chapterRepository = chapterRepository;
        _analyzer = analyzer;
        _cacheService = cacheService;
        _logger = logger;
        _coursePersistenceService = coursePersistenceService;
    }

    public async Task<UploadCoursesResponse> UploadManyAsync(
        Guid userId, IEnumerable<UploadFileDto> files, string title, CancellationToken ct = default)
    {
        var fileList = files.ToList();
        var (valid, errors) = ValidateFiles(fileList);

        if (valid.Count == 0)
            return new UploadCoursesResponse([], fileList.Count, 0, fileList.Count, errors);

        var uploads = new List<CourseUpload>();
        var jobs = new List<CourseJob>();

        foreach (var file in valid)
        {
            var (fileName, fileKey, extension) = await UploadSingleAsync(file, ct);
            var upload = CourseUpload.Create(userId, title, fileName, fileKey, extension);
            var job = CourseJob.Create(upload.Id, "pending");

            uploads.Add(upload);
            jobs.Add(job);
        }

        await _courseUploadsRepository.AddRangeAsync(uploads, ct);
        await _courseJobRepository.AddRangeAsync(jobs, ct);
        await _uow.SaveChangesAsync(ct);

        foreach (var job in jobs)
            _jobClient.Enqueue<ProcessCourseJob>(p => p.ExecuteAsync(job.Id, userId, CancellationToken.None));

        var summaries = jobs
            .Zip(uploads, (job, upload) => new CourseJobSummaryDto(job.Id, upload.FileName, "Pending"))
            .ToList();

        return new UploadCoursesResponse(summaries, fileList.Count, valid.Count, errors.Count, errors);
    }

    public async Task FinishChapterAsync(Guid userId, Guid courseId, Guid chapterId, CancellationToken ct = default)
    {
        var profile = await _userProfileRepository.GetCurrentUserProfile(userId, ct)
                      ?? throw new UserProfileNotFoundException($"Profilul cu id-ul {userId} nu a fost gasit");

        if (!await _chapterRepository.ExistsAsync(chapterId, courseId, ct))
            throw new ChapterNotFoundException($"Capitolul {chapterId} nu a fost gasit in cursul {courseId}");

        if (!profile.Courses.Any(c => c.Id == courseId))
            throw new InvalidFieldException("Nu esti inscris in acest curs.");

        profile.FinishChapter(chapterId.ToString());
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<CourseSkeletonResponse> GetCourseSkeletonAsync(
        CourseSkeletonRequest request, CancellationToken ct = default)
    {
        var cacheKey = $"course:skeleton:{request.Topic}";
        var cached = await _cacheService.GetAsync<CourseSkeletonResponse>(cacheKey, ct);

        if (cached is not null)
        {
            _logger.LogInformation("Cache hit for skeleton of {Topic}", request.Topic);
            return cached;
        }

        var response = await _analyzer.GetCourseSkeletonAsync(request, ct);
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromHours(2), ct);
        return response;
    }

    public async Task<Guid> SaveCourseContentAsync(
        Guid userId, string topic, CourseSkeletonResponse response, CancellationToken ct = default)
    {
        var expandedChapters = await GetOrCacheExpandedChaptersAsync(topic, response, ct);
        return await _coursePersistenceService.PersistAsync(userId, topic, expandedChapters.ToAssemblyModels(), ct);
    }

    private async Task<List<ChapterExpandResponse>> GetOrCacheExpandedChaptersAsync(
        string topic, CourseSkeletonResponse response, CancellationToken ct)
    {
        var cacheKey = $"course:expand:{topic}";
        var cached = await _cacheService.GetAsync<List<ChapterExpandResponse>>(cacheKey, ct);

        if (cached is not null)
        {
            _logger.LogInformation("Cache hit for chapter expand of {Topic}", topic);
            return cached;
        }

        var concurrentChapters = new ConcurrentBag<ChapterExpandResponse>();
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 6, CancellationToken = ct };

        await Parallel.ForEachAsync(response.Skeleton.Chapters, parallelOptions, async (chapterSkeleton, token) =>
        {
            var request = new ChapterRequest(topic, chapterSkeleton.Title, chapterSkeleton.CoreConcept);
            concurrentChapters.Add(await _analyzer.GetExpandedChapterAsync(request, token));
        });

        var result = concurrentChapters.ToList();
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromHours(2), ct);
        return result;
    }

    private static (List<UploadFileDto> Valid, List<string> Errors) ValidateFiles(List<UploadFileDto> files)
    {
        var valid = new List<UploadFileDto>();
        var errors = new List<string>();

        foreach (var file in files)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (file.Content.Length == 0)
                errors.Add($"{file.FileName}: file is empty.");
            else if (!CourseConstants.AllowedExtensions.Contains(extension))
                errors.Add($"{file.FileName}: only PDF, DOC and DOCX files are allowed.");
            else if (file.Content.Length > CourseConstants.MaxFileSizeBytes)
                errors.Add($"{file.FileName}: exceeds 20MB limit.");
            else if (valid.Count >= CourseConstants.MaxFiles)
                errors.Add($"{file.FileName}: max {CourseConstants.MaxFiles} files per request.");
            else
                valid.Add(file);
        }

        return (valid, errors);
    }

    private async Task<(string FileName, string FileKey, string Extension)> UploadSingleAsync(
        UploadFileDto file, CancellationToken ct)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var key = await _storage.UploadAsync(file.Content, file.FileName, file.ContentType, ct);
        return (file.FileName, key, extension);
    }
}