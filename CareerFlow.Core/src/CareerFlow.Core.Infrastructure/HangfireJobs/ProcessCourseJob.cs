using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.Models.AI;
using CareerFlow.Core.Domain.ValueObjects;
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
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger<ProcessCourseJob> _logger;
    private readonly IQuizRepository _quizRepository;
    private readonly IStorageService _storage;
    private readonly IUnitOfWork _uow;
    private readonly IUserProfileRepository _userProfileRepository;

    public ProcessCourseJob(
        IUnitOfWork uow,
        IStorageService storage,
        IDocumentAnalyzerService analyzer,
        ILogger<ProcessCourseJob> logger,
        ICourseJobRepository courseJobRepository,
        ICourseRepository courseRepository,
        IQuizRepository quizRepository,
        IUserProfileRepository userProfileRepository,
        ICacheService cache)
    {
        ArgumentNullException.ThrowIfNull(uow);
        ArgumentNullException.ThrowIfNull(storage);
        ArgumentNullException.ThrowIfNull(analyzer);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(courseJobRepository);
        ArgumentNullException.ThrowIfNull(courseRepository);
        ArgumentNullException.ThrowIfNull(quizRepository);
        ArgumentNullException.ThrowIfNull(userProfileRepository);
        ArgumentNullException.ThrowIfNull(cache);

        _uow = uow;
        _storage = storage;
        _analyzer = analyzer;
        _logger = logger;
        _courseJobRepository = courseJobRepository;
        _courseRepository = courseRepository;
        _userProfileRepository = userProfileRepository;
        _quizRepository = quizRepository;
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

        await UpdateStatusAsync(job, JobStatus.Processing, ct);

        try
        {
            var cacheKey = $"course:full:{job.Upload!.FileKey}";
            var cached = await _cache.GetAsync<FullCourseResponse>(cacheKey, ct);

            FullCourseResponse fullResult;
            if (cached is not null)
            {
                _logger.LogInformation("Cache hit for {FileKey}", job.Upload.FileKey);
                fullResult = cached;
            }
            else
            {
                var fileBytes = await DownloadFileBytesAsync(job.Upload!.FileKey, ct);
                var fileName = job.Upload!.FileName;

                var formFile = CreateFormFile(fileBytes, fileName);
                fullResult = await _analyzer.GenerateFullCourseAsync(formFile, 7, ct);

                await _cache.SetAsync(cacheKey, fullResult, TimeSpan.FromHours(2), ct);
            }

            var topic = job.Upload!.Title;
            var chapters = BuildChaptersFromResponse(fullResult);
            var course = Course.Create(T(topic), chapters);

            foreach (var chapter in course.Chapters) chapter.SetCourseId(course.Id);
            var userProfile = await _userProfileRepository.GetCurrentUserProfile(userId, ct) ??
                              throw new UserProfileNotFoundException($"Profilul cu id-ul {userId} nu a fost gasit");
            userProfile.EnrollInCourse(course);
            await _uow.BeginTransactionAsync(ct);

            try
            {
                await _courseRepository.AddAsync(course, ct);
                _userProfileRepository.Update(userProfile);
                await _uow.SaveChangesAsync(ct);

                var quizQuestions = BuildQuizQuestions(fullResult, course);

                foreach (var question in quizQuestions) await _quizRepository.AddAsync(question, ct);

                job.SetCourseId(course.Id);
                await UpdateStatusAsync(job, JobStatus.Done, ct);

                await _uow.CommitAsync(ct);

                _logger.LogInformation("Job {JobId} done. Course {CourseId}", jobId, course.Id);
            }
            catch
            {
                await _uow.RollbackAsync(ct);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job {JobId} failed", jobId);
            job.SetErrorMessage(ex.Message);
            await UpdateStatusAsync(job, JobStatus.Failed, ct);
            throw;
        }
    }

    private static List<Chapter> BuildChaptersFromResponse(FullCourseResponse fullResult)
    {
        return fullResult.Chapters.Select(ch =>
        {
            var subChapterEntities = ch.Subchapters
                .Select(s => SubChapter.Create(T(s.Title), T(s.Sumary), s.TheoryHtml))
                .ToList();

            return Chapter.Create(
                ch.Day,
                T(ch.ChapterTitle),
                T(ch.CoreConcept),
                subChapterEntities);
        }).ToList();
    }

    private static List<QuizQuestion> BuildQuizQuestions(FullCourseResponse fullResult, Course course)
    {
        var questions = new List<QuizQuestion>();

        foreach (var (chData, chapter) in fullResult.Chapters.Zip(course.Chapters))
        {
            questions.AddRange(chData.Quiz.Quiz.Select(q =>
                QuizQuestion.Create(
                    T(q.Question, 500),
                    q.Options.Select(o => T(o)).ToList(),
                    T(q.CorrectAnswer),
                    chapter.Id,
                    null)));

            foreach (var (subContent, subChapter) in chData.Subchapters.Zip(chapter.SubChapters))
                questions.AddRange(subContent.MiniQuiz.Select(q =>
                    QuizQuestion.Create(
                        T(q.Question, 500),
                        q.Options.Select(o => T(o)).ToList(),
                        T(q.CorrectAnswer),
                        null,
                        subChapter.Id)));
        }

        return questions;
    }

    private static string T(string? value, int max = 200)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= max)
            return value ?? "";

        var truncated = value[..max];
        var lastSpace = truncated.LastIndexOf(' ');
        return lastSpace > 0 ? truncated[..lastSpace] : truncated;
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

    private async Task UpdateStatusAsync(CourseJob job, JobStatus status, CancellationToken ct)
    {
        job.Update(status);
        await _uow.SaveChangesAsync(ct);
    }
}