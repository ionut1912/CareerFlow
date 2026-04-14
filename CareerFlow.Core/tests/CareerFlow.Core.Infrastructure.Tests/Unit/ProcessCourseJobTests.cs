using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Models.AI.Dto;
using CareerFlow.Core.Domain.Models.AI.Responses;
using CareerFlow.Core.Domain.Models.Course.Dto;
using CareerFlow.Core.Domain.ValueObjects;
using CareerFlow.Core.Infrastructure.HangfireJobs;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Unit;

public class ProcessCourseJobTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IStorageService> _storageMock = new();
    private readonly Mock<IDocumentAnalyzerService> _analyzerMock = new();
    private readonly Mock<ILogger<ProcessCourseJob>> _loggerMock = new();
    private readonly Mock<ICourseJobRepository> _jobRepoMock = new();
    private readonly Mock<ICoursePersistenceService> _persistenceMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly ProcessCourseJob _sut;

    public ProcessCourseJobTests()
    {
        _sut = new ProcessCourseJob(
            _uowMock.Object, _storageMock.Object, _analyzerMock.Object,
            _loggerMock.Object, _jobRepoMock.Object, _persistenceMock.Object, _cacheMock.Object);
    }

    [Fact]
    public void Constructor_NullUow_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => new ProcessCourseJob(
            null!, _storageMock.Object, _analyzerMock.Object, _loggerMock.Object,
            _jobRepoMock.Object, _persistenceMock.Object, _cacheMock.Object));
    }

    [Fact]
    public void Constructor_NullStorage_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => new ProcessCourseJob(
            _uowMock.Object, null!, _analyzerMock.Object, _loggerMock.Object,
            _jobRepoMock.Object, _persistenceMock.Object, _cacheMock.Object));
    }

    [Fact]
    public void Constructor_NullAnalyzer_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => new ProcessCourseJob(
            _uowMock.Object, _storageMock.Object, null!, _loggerMock.Object,
            _jobRepoMock.Object, _persistenceMock.Object, _cacheMock.Object));
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => new ProcessCourseJob(
            _uowMock.Object, _storageMock.Object, _analyzerMock.Object, null!,
            _jobRepoMock.Object, _persistenceMock.Object, _cacheMock.Object));
    }

    [Fact]
    public void Constructor_NullJobRepository_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => new ProcessCourseJob(
            _uowMock.Object, _storageMock.Object, _analyzerMock.Object, _loggerMock.Object,
            null!, _persistenceMock.Object, _cacheMock.Object));
    }

    [Fact]
    public void Constructor_NullPersistence_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => new ProcessCourseJob(
            _uowMock.Object, _storageMock.Object, _analyzerMock.Object, _loggerMock.Object,
            _jobRepoMock.Object, null!, _cacheMock.Object));
    }

    [Fact]
    public void Constructor_NullCache_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => new ProcessCourseJob(
            _uowMock.Object, _storageMock.Object, _analyzerMock.Object, _loggerMock.Object,
            _jobRepoMock.Object, _persistenceMock.Object, null!));
    }

    [Fact]
    public async Task ExecuteAsync_JobNotFound_LogsWarning()
    {
        var jobId = Guid.NewGuid();
        _jobRepoMock.Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<CourseJob, object>>[]>()))
            .ReturnsAsync((CourseJob?)null);

        await _sut.ExecuteAsync(jobId, Guid.NewGuid(), CancellationToken.None);

        _loggerMock.Verify(
            x => x.Log(LogLevel.Warning, It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains(jobId.ToString())),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_JobNotFound_DoesNotCallStorage()
    {
        _jobRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<CourseJob, object>>[]>()))
            .ReturnsAsync((CourseJob?)null);

        await _sut.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        _storageMock.Verify(s => s.DownloadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_AnalyzerThrows_SetsJobToFailed()
    {
        var (job, jobId, userId) = SetupJobWithUpload();
        SetupCacheMiss();
        _storageMock.Setup(s => s.DownloadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream(new byte[] { 1 }));
        _analyzerMock
            .Setup(a => a.AnalyzeDocumentAsync(It.IsAny<UploadFileDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("AI down"));
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await Should.ThrowAsync<HttpRequestException>(() =>
            _sut.ExecuteAsync(jobId, userId, CancellationToken.None));

        job.Status.ShouldBe(JobStatus.Failed);
    }

    [Fact]
    public async Task ExecuteAsync_AnalyzerThrows_SetsErrorMessage()
    {
        var (job, jobId, userId) = SetupJobWithUpload();
        _cacheMock.Setup(c => c.GetAsync<DocumentProcessingResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DocumentProcessingResponse)null!);
        _storageMock.Setup(s => s.DownloadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream(new byte[] { 1 }));
        _analyzerMock
            .Setup(a => a.AnalyzeDocumentAsync(It.IsAny<UploadFileDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("AI service down"));
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var exception = await Should.ThrowAsync<HttpRequestException>(() =>
            _sut.ExecuteAsync(jobId, userId, CancellationToken.None));

        exception.Message.ShouldBe("AI service down");
        job.ErrorMessage!.ShouldContain("AI service down");
        job.Status.ShouldBe(JobStatus.Failed);
    }

    [Fact]
    public async Task ExecuteAsync_DocumentCacheHit_SkipsStorage()
    {
        var (_, jobId, userId) = SetupJobWithUpload();
        var docResponse = BuildDocumentResponse();
        _cacheMock.Setup(c => c.GetAsync<DocumentProcessingResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(docResponse);
        _cacheMock.Setup(c => c.GetAsync<List<ExpandedChapterDataDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExpandedChapterDataDto>());
        _persistenceMock.Setup(p => p.PersistAsync(userId, It.IsAny<string>(),
                It.IsAny<List<CareerFlow.Core.Domain.Models.Assembly.ChapterAssemblyModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _sut.ExecuteAsync(jobId, userId, CancellationToken.None);

        _storageMock.Verify(s => s.DownloadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_SuccessfulExecution_SetsCourseIdOnJob()
    {
        var courseId = Guid.NewGuid();
        var (job, jobId, userId) = SetupJobWithUpload();
        _cacheMock.Setup(c => c.GetAsync<DocumentProcessingResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildDocumentResponse());
        _cacheMock.Setup(c => c.GetAsync<List<ExpandedChapterDataDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExpandedChapterDataDto>());
        _persistenceMock.Setup(p => p.PersistAsync(userId, It.IsAny<string>(),
                It.IsAny<List<CareerFlow.Core.Domain.Models.Assembly.ChapterAssemblyModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseId);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _sut.ExecuteAsync(jobId, userId, CancellationToken.None);

        job.CourseId.ShouldBe(courseId);
        job.Status.ShouldBe(JobStatus.Done);
    }

    [Fact]
    public async Task ExecuteAsync_SuccessfulExecution_CallsPersistenceService()
    {
        var (_, jobId, userId) = SetupJobWithUpload();
        _cacheMock.Setup(c => c.GetAsync<DocumentProcessingResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildDocumentResponse());
        _cacheMock.Setup(c => c.GetAsync<List<ExpandedChapterDataDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExpandedChapterDataDto>());
        _persistenceMock.Setup(p => p.PersistAsync(userId, It.IsAny<string>(),
                It.IsAny<List<CareerFlow.Core.Domain.Models.Assembly.ChapterAssemblyModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _sut.ExecuteAsync(jobId, userId, CancellationToken.None);

        _persistenceMock.Verify(p => p.PersistAsync(userId, It.IsAny<string>(),
            It.IsAny<List<CareerFlow.Core.Domain.Models.Assembly.ChapterAssemblyModel>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private (CourseJob Job, Guid JobId, Guid UserId) SetupJobWithUpload()
    {
        var jobId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var upload = CourseUpload.Create(userId, "Course", "file.pdf", "storage/file.pdf", "pdf");
        var job = CourseJob.Create(upload.Id, "pending");

        typeof(CourseJob).GetProperty(nameof(CourseJob.Upload))?.SetValue(job, upload);

        _jobRepoMock.Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<CourseJob, object>>[]>()))
            .ReturnsAsync(job);

        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        return (job, jobId, userId);
    }

    private void SetupCacheMiss()
    {
        _cacheMock.Setup(c => c.GetAsync<DocumentProcessingResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DocumentProcessingResponse?)null);
        _cacheMock.Setup(c => c.GetAsync<List<ExpandedChapterDataDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<ExpandedChapterDataDto>?)null);
    }

    private static DocumentProcessingResponse BuildDocumentResponse()
    {
        var analysis = new DocumentAnalysisDto("Test Title", "Test Summary", ["Topic 1", "Topic 2"]);
        var skeleton = new SkeletonDto("Test Topic", []);

        return new DocumentProcessingResponse(
            Guid.NewGuid().ToString(),
            analysis,
            skeleton,
            5);
    }
}