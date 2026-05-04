using System.Linq.Expressions;

using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Models.AI.Dto;
using CareerFlow.Core.Domain.Models.AI.Responses;
using CareerFlow.Core.Domain.Models.Assembly;
using CareerFlow.Core.Domain.Models.Course.Dto;
using CareerFlow.Core.Domain.ValueObjects;
using CareerFlow.Core.Infrastructure.HangfireJobs;
using CareerFlow.Core.Infrastructure.Tests.Unit.Setup;

using Microsoft.Extensions.Logging;

using Moq;

using Shouldly;

using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Unit.HangfireJobs;

public class ProcessCourseJobTests
{
    private readonly Mock<IDocumentAnalyzerService> _analyzerMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();

    // FakeLogger captures structured log records produced by [LoggerMessage] source-generated
    // methods. Mock<ILogger<T>> cannot intercept them: the generator creates a private TState
    // struct that Moq's It.IsAnyType matching cannot resolve even after IsEnabled returns true.
    private readonly FakeLogger<ProcessCourseJob> _fakeLogger = new();
    private readonly Mock<ICourseJobRepository> _jobRepoMock = new();

    private readonly Mock<ICoursePersistenceService> _persistenceMock = new();
    private readonly Mock<IStorageService> _storageMock = new();
    private readonly ProcessCourseJob _sut;
    private readonly Mock<IUnitOfWork> _uowMock = new();

    public ProcessCourseJobTests()
    {
        _sut = new ProcessCourseJob(
            _uowMock.Object, _storageMock.Object, _analyzerMock.Object,
            _fakeLogger, _jobRepoMock.Object, _persistenceMock.Object, _cacheMock.Object);
    }

    // ── Constructor ──────────────────────────────────────────────────────────
    // Single-line guard checks; AAA adds no value here.

    [Fact]
    public void Constructor_NullUow_ThrowsArgumentNullException() =>
        Should.Throw<ArgumentNullException>(() => new ProcessCourseJob(
            null!, _storageMock.Object, _analyzerMock.Object, _fakeLogger,
            _jobRepoMock.Object, _persistenceMock.Object, _cacheMock.Object));

    [Fact]
    public void Constructor_NullStorage_ThrowsArgumentNullException() =>
        Should.Throw<ArgumentNullException>(() => new ProcessCourseJob(
            _uowMock.Object, null!, _analyzerMock.Object, _fakeLogger,
            _jobRepoMock.Object, _persistenceMock.Object, _cacheMock.Object));

    [Fact]
    public void Constructor_NullAnalyzer_ThrowsArgumentNullException() =>
        Should.Throw<ArgumentNullException>(() => new ProcessCourseJob(
            _uowMock.Object, _storageMock.Object, null!, _fakeLogger,
            _jobRepoMock.Object, _persistenceMock.Object, _cacheMock.Object));

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException() =>
        Should.Throw<ArgumentNullException>(() => new ProcessCourseJob(
            _uowMock.Object, _storageMock.Object, _analyzerMock.Object, null!,
            _jobRepoMock.Object, _persistenceMock.Object, _cacheMock.Object));

    [Fact]
    public void Constructor_NullJobRepository_ThrowsArgumentNullException() =>
        Should.Throw<ArgumentNullException>(() => new ProcessCourseJob(
            _uowMock.Object, _storageMock.Object, _analyzerMock.Object, _fakeLogger,
            null!, _persistenceMock.Object, _cacheMock.Object));

    [Fact]
    public void Constructor_NullPersistence_ThrowsArgumentNullException() =>
        Should.Throw<ArgumentNullException>(() => new ProcessCourseJob(
            _uowMock.Object, _storageMock.Object, _analyzerMock.Object, _fakeLogger,
            _jobRepoMock.Object, null!, _cacheMock.Object));

    [Fact]
    public void Constructor_NullCache_ThrowsArgumentNullException() =>
        Should.Throw<ArgumentNullException>(() => new ProcessCourseJob(
            _uowMock.Object, _storageMock.Object, _analyzerMock.Object, _fakeLogger,
            _jobRepoMock.Object, _persistenceMock.Object, null!));

    // ── ExecuteAsync — job not found ─────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_JobNotFound_LogsWarning()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        _jobRepoMock.Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<CourseJob, object>>[]>()))
            .ReturnsAsync((CourseJob?)null);

        // Act
        await _sut.ExecuteAsync(jobId, Guid.NewGuid(), CancellationToken.None);

        // Assert
        FakeLogRecord record = _fakeLogger.Records.ShouldHaveSingleItem();
        record.Level.ShouldBe(LogLevel.Warning);
        record.Message.ShouldContain(jobId.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_JobNotFound_DoesNotCallStorage()
    {
        // Arrange
        _jobRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<CourseJob, object>>[]>()))
            .ReturnsAsync((CourseJob?)null);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        _storageMock.Verify(s => s.DownloadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_JobNotFound_DoesNotSaveChanges()
    {
        // Arrange
        _jobRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<CourseJob, object>>[]>()))
            .ReturnsAsync((CourseJob?)null);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── ExecuteAsync — failure path ──────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_AnalyzerThrows_SetsJobToFailed()
    {
        // Arrange
        (CourseJob job, Guid jobId, Guid userId) = SetupJobWithUpload();
        SetupCacheMiss();
        _storageMock.Setup(s => s.DownloadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream([1]));
        _analyzerMock
            .Setup(a => a.AnalyzeDocumentAsync(It.IsAny<UploadFileDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("AI down"));

        // Act
        await Should.ThrowAsync<HttpRequestException>(() =>
            _sut.ExecuteAsync(jobId, userId, CancellationToken.None));

        // Assert
        job.Status.ShouldBe(JobStatus.Failed);
    }

    [Fact]
    public async Task ExecuteAsync_AnalyzerThrows_SetsErrorMessage()
    {
        // Arrange
        (CourseJob job, Guid jobId, Guid userId) = SetupJobWithUpload();
        SetupCacheMiss();
        _storageMock.Setup(s => s.DownloadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream([1]));
        _analyzerMock
            .Setup(a => a.AnalyzeDocumentAsync(It.IsAny<UploadFileDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("AI service down"));

        // Act
        HttpRequestException exception = await Should.ThrowAsync<HttpRequestException>(() =>
            _sut.ExecuteAsync(jobId, userId, CancellationToken.None));

        // Assert
        exception.Message.ShouldBe("AI service down");
        job.ErrorMessage.ShouldNotBeNull();
        job.ErrorMessage.ShouldContain("AI service down");
    }

    [Fact]
    public async Task ExecuteAsync_AnalyzerThrows_RethrowsException()
    {
        // Arrange
        (_, Guid jobId, Guid userId) = SetupJobWithUpload();
        SetupCacheMiss();
        _storageMock.Setup(s => s.DownloadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream([1]));
        _analyzerMock
            .Setup(a => a.AnalyzeDocumentAsync(It.IsAny<UploadFileDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("AI down"));

        // Act & Assert — the catch block must re-throw so Hangfire can retry
        await Should.ThrowAsync<HttpRequestException>(() =>
            _sut.ExecuteAsync(jobId, userId, CancellationToken.None));
    }

    // ── ExecuteAsync — cache behaviour ──────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_DocumentCacheHit_SkipsStorage()
    {
        // Arrange
        (_, Guid jobId, Guid userId) = SetupJobWithUpload();
        SetupSuccessPath(userId);

        // Act
        await _sut.ExecuteAsync(jobId, userId, CancellationToken.None);

        // Assert
        _storageMock.Verify(s => s.DownloadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_DocumentCacheMiss_DownloadsFromStorage()
    {
        // Arrange
        (_, Guid jobId, Guid userId) = SetupJobWithUpload();
        SetupCacheMiss();
        _storageMock.Setup(s => s.DownloadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream([1]));
        _analyzerMock
            .Setup(a => a.AnalyzeDocumentAsync(It.IsAny<UploadFileDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildDocumentResponse());
        _persistenceMock.Setup(p => p.PersistAsync(userId, It.IsAny<string>(),
                It.IsAny<List<ChapterAssemblyModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        await _sut.ExecuteAsync(jobId, userId, CancellationToken.None);

        // Assert
        _storageMock.Verify(s => s.DownloadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DocumentCacheMiss_CallsAnalyzer()
    {
        // Arrange
        (_, Guid jobId, Guid userId) = SetupJobWithUpload();
        SetupCacheMiss();
        _storageMock.Setup(s => s.DownloadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream([1]));
        _analyzerMock
            .Setup(a => a.AnalyzeDocumentAsync(It.IsAny<UploadFileDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildDocumentResponse());
        _persistenceMock.Setup(p => p.PersistAsync(userId, It.IsAny<string>(),
                It.IsAny<List<ChapterAssemblyModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        await _sut.ExecuteAsync(jobId, userId, CancellationToken.None);

        // Assert
        _analyzerMock.Verify(
            a => a.AnalyzeDocumentAsync(It.IsAny<UploadFileDto>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── ExecuteAsync — success path ──────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_ValidJob_SetsJobToProcessing()
    {
        // Arrange
        (CourseJob job, Guid jobId, Guid userId) = SetupJobWithUpload();
        SetupSuccessPath(userId);

        // Act
        await _sut.ExecuteAsync(jobId, userId, CancellationToken.None);

        // Assert — StartedAt is set exclusively by the Processing transition
        job.StartedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_SuccessfulExecution_SetsCourseIdOnJob()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        (CourseJob job, Guid jobId, Guid userId) = SetupJobWithUpload();
        _cacheMock.Setup(c => c.GetAsync<DocumentProcessingResponse>(It.IsAny<string>()))
            .ReturnsAsync(BuildDocumentResponse());
        _cacheMock.Setup(c => c.GetAsync<List<ExpandedChapterDataDto>>(It.IsAny<string>()))
            .ReturnsAsync([]);
        _persistenceMock.Setup(p => p.PersistAsync(userId, It.IsAny<string>(),
                It.IsAny<List<ChapterAssemblyModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseId);

        // Act
        await _sut.ExecuteAsync(jobId, userId, CancellationToken.None);

        // Assert
        job.CourseId.ShouldBe(courseId);
        job.Status.ShouldBe(JobStatus.Done);
    }

    [Fact]
    public async Task ExecuteAsync_SuccessfulExecution_CallsPersistenceService()
    {
        // Arrange
        (_, Guid jobId, Guid userId) = SetupJobWithUpload();
        SetupSuccessPath(userId);

        // Act
        await _sut.ExecuteAsync(jobId, userId, CancellationToken.None);

        // Assert
        _persistenceMock.Verify(p => p.PersistAsync(userId, It.IsAny<string>(),
                It.IsAny<List<ChapterAssemblyModel>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_SuccessfulExecution_SavesChangesForEachStatusTransition()
    {
        // Arrange
        (_, Guid jobId, Guid userId) = SetupJobWithUpload();
        SetupSuccessPath(userId);

        // Act
        await _sut.ExecuteAsync(jobId, userId, CancellationToken.None);

        // Assert — Processing → Done means at least two SaveChanges calls
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private (CourseJob Job, Guid JobId, Guid UserId) SetupJobWithUpload()
    {
        var jobId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var upload = CourseUpload.Create(userId, "Course", "file.pdf", "storage/file.pdf", "pdf");
        var job = CourseJob.Create(upload.Id, "pending");

        // Upload has a private setter; reflection is the only option without changing the entity.
        typeof(CourseJob).GetProperty(nameof(CourseJob.Upload))!
            .SetValue(job, upload);

        _jobRepoMock.Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<CourseJob, object>>[]>()))
            .ReturnsAsync(job);

        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        return (job, jobId, userId);
    }

    private void SetupCacheMiss()
    {
        _cacheMock.Setup(c => c.GetAsync<DocumentProcessingResponse>(It.IsAny<string>()))
            .ReturnsAsync((DocumentProcessingResponse?)null);
        _cacheMock.Setup(c => c.GetAsync<List<ExpandedChapterDataDto>>(It.IsAny<string>()))
            .ReturnsAsync((List<ExpandedChapterDataDto>?)null);
    }

    // Shared success-path setup: both cache layers return hits so no storage/analyzer
    // calls are made, and persistence always returns a new course id.
    private void SetupSuccessPath(Guid userId)
    {
        _cacheMock.Setup(c => c.GetAsync<DocumentProcessingResponse>(It.IsAny<string>()))
            .ReturnsAsync(BuildDocumentResponse());
        _cacheMock.Setup(c => c.GetAsync<List<ExpandedChapterDataDto>>(It.IsAny<string>()))
            .ReturnsAsync([]);
        _persistenceMock.Setup(p => p.PersistAsync(userId, It.IsAny<string>(),
                It.IsAny<List<ChapterAssemblyModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());
    }

    private static DocumentProcessingResponse BuildDocumentResponse()
    {
        var analysis = new DocumentAnalysisDto("Test Title", "Test Summary", ["Topic 1", "Topic 2"]);
        var skeleton = new SkeletonDto("Test Topic", []);
        return new DocumentProcessingResponse(Guid.NewGuid().ToString(), analysis, skeleton, 5);
    }
}
