using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Requests;
using CareerFlow.Core.Application.Responses;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Constants;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Infrastructure.HangfireJobs;
using Hangfire;
using Microsoft.AspNetCore.Http;

namespace CareerFlow.Core.Infrastructure.Services;

public sealed class CourseService : ICourseService
{
    private readonly IChapterRepository _chapterRepository;
    private readonly ICourseJobRepository _courseJobRepository;
    private readonly ICourseUploadsRepository _courseUploadsRepository;
    private readonly IBackgroundJobClient _jobClient;
    private readonly IStorageService _storage;
    private readonly IUnitOfWork _uow;
    private readonly IUserProfileRepository _userProfileRepository;

    public CourseService(
        IStorageService storage,
        IBackgroundJobClient jobClient,
        IUnitOfWork uow,
        ICourseUploadsRepository courseUploadsRepository,
        ICourseJobRepository courseJobRepository,
        IUserProfileRepository userProfileRepository,
        IChapterRepository chapterRepository)
    {
        ArgumentNullException.ThrowIfNull(storage);
        ArgumentNullException.ThrowIfNull(jobClient);
        ArgumentNullException.ThrowIfNull(uow);
        ArgumentNullException.ThrowIfNull(courseUploadsRepository);
        ArgumentNullException.ThrowIfNull(courseJobRepository);
        ArgumentNullException.ThrowIfNull(userProfileRepository);
        ArgumentNullException.ThrowIfNull(chapterRepository);

        _storage = storage;
        _jobClient = jobClient;
        _uow = uow;
        _courseUploadsRepository = courseUploadsRepository;
        _courseJobRepository = courseJobRepository;
        _userProfileRepository = userProfileRepository;
        _chapterRepository = chapterRepository;
    }

    public async Task<UploadCoursesResponse> UploadManyAsync(
        Guid userId, UploadCoursesRequest request, CancellationToken ct = default)
    {
        var valid = new List<IFormFile>();
        var errors = new List<string>();

        foreach (var file in request.Files)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (file.Length == 0)
                errors.Add($"{file.FileName}: file is empty.");
            else if (!CourseConstants.AllowedExtensions.Contains(extension))
                errors.Add($"{file.FileName}: only PDF, DOC and DOCX files are allowed.");
            else if (file.Length > CourseConstants.MaxFileSizeBytes)
                errors.Add($"{file.FileName}: exceeds 20MB limit.");
            else if (valid.Count >= CourseConstants.MaxFiles)
                errors.Add($"{file.FileName}: max {CourseConstants.MaxFiles} files per request.");
            else
                valid.Add(file);
        }

        if (valid.Count == 0)
            return new UploadCoursesResponse([], request.Files.Count, 0, request.Files.Count, errors);

        var uploads = new List<CourseUpload>();
        var jobs = new List<CourseJob>();

        foreach (var file in valid)
        {
            var (fileName, fileKey, extension) = await UploadSingleAsync(file, ct);
            var upload = CourseUpload.Create(userId, request.Title, fileName, fileKey, extension);
            var job = CourseJob.Create(upload.Id, "pending");

            uploads.Add(upload);
            jobs.Add(job);
        }

        await _courseUploadsRepository.AddRangeAsync(uploads, ct);
        await _courseJobRepository.AddRangeAsync(jobs, ct);
        await _uow.SaveChangesAsync(ct);

        foreach (var job in jobs)
            _jobClient.Enqueue<ProcessCourseJob>(p => p.ExecuteAsync(job.Id, userId, CancellationToken.None));

        var summaries = jobs.Zip(uploads, (job, upload) =>
                new CourseJobSummaryDto(job.Id, upload.FileName, "Pending"))
            .ToList();

        return new UploadCoursesResponse(summaries, request.Files.Count, valid.Count, errors.Count, errors);
    }

    public async Task<IEnumerable<CourseJobStatusResponse>> GetJobStatusesAsync(
        Guid[] jobIds, CancellationToken ct = default)
    {
        return await _courseJobRepository.GetJobStatusesAsync(jobIds, ct);
    }

    public async Task FinishChapterAsync(Guid userId, Guid courseId, Guid chapterId, CancellationToken ct = default)
    {
        var profile = await _userProfileRepository.GetCurrentUserProfile(userId, ct)
                      ?? throw new UserProfileNotFoundException($"Profilul cu id-ul {userId} nu a fost gasit");

        var chapterExists = await _chapterRepository.ExistsAsync(chapterId, courseId, ct);
        if (!chapterExists)
            throw new ChapterNotFoundException($"Capitolul {chapterId} nu a fost gasit in cursul {courseId}");

        var isEnrolled = profile.Courses.Any(c => c.Id == courseId);
        if (!isEnrolled)
            throw new InvalidFieldException("Nu esti inscris in acest curs.");

        profile.FinishChapter(chapterId.ToString());
        await _uow.SaveChangesAsync(ct);
    }

    private async Task<(string FileName, string FileKey, string Extension)> UploadSingleAsync(
        IFormFile file, CancellationToken ct)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        await using var stream = file.OpenReadStream();
        var key = await _storage.UploadAsync(stream, file.FileName, file.ContentType, ct);
        return (file.FileName, key, extension);
    }
}