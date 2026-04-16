using System.Reflection;
using System.Runtime.CompilerServices;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Constants;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.Models.AI.Dto;
using CareerFlow.Core.Domain.Models.AI.Requests;
using CareerFlow.Core.Domain.Models.AI.Responses;
using CareerFlow.Core.Domain.Models.Assembly;
using CareerFlow.Core.Domain.Models.Course.Dto;
using CareerFlow.Core.Domain.ValueObjects;
using CareerFlow.Core.Infrastructure.Services;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Domain.Common;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Unit.Services;

public class CourseServiceTests
{
    private readonly Mock<IAnalyzerService> _analyzer = new();
    private readonly Mock<ICacheService> _cacheService = new();
    private readonly Mock<IChapterRepository> _chapterRepository = new();
    private readonly Mock<ICourseJobRepository> _courseJobRepository = new();
    private readonly Mock<ICoursePersistenceService> _coursePersistenceService = new();
    private readonly Mock<ICourseUploadsRepository> _courseUploadsRepository = new();
    private readonly Mock<IBackgroundJobClient> _jobClient = new();
    private readonly Mock<ILogger<CourseService>> _logger = new();
    private readonly Mock<IStorageService> _storage = new();

    private readonly CourseService _sut;
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IUserProfileRepository> _userProfileRepository = new();

    public CourseServiceTests()
    {
        _sut = new CourseService(
            _storage.Object,
            _jobClient.Object,
            _logger.Object,
            _uow.Object,
            _cacheService.Object,
            _courseUploadsRepository.Object,
            _courseJobRepository.Object,
            _userProfileRepository.Object,
            _chapterRepository.Object,
            _analyzer.Object,
            _coursePersistenceService.Object
        );
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static UploadFileDto CreateFile(
        string fileName = "test.pdf",
        string contentType = "application/pdf",
        byte[]? content = null)
    {
        return new UploadFileDto(fileName, contentType, new MemoryStream(content ?? [1, 2, 3]));
    }

    private static UploadFileDto EmptyFile(string fileName = "empty.pdf")
    {
        return new UploadFileDto(fileName, "application/pdf", new MemoryStream([]));
    }

    private static UploadFileDto OversizedFile(string fileName = "big.pdf")
    {
        return new UploadFileDto(fileName, "application/pdf",
            new MemoryStream(new byte[CourseConstants.MaxFileSizeBytes + 1]));
    }

    private static UploadFileDto DisallowedFile(string fileName = "virus.exe")
    {
        return new UploadFileDto(fileName, "application/octet-stream", new MemoryStream([1, 2, 3]));
    }

    private static UserProfile CreateProfile(Guid userId)
    {
        return UserProfile.Create(userId, LearningType.Visual, [UserType.Student]);
    }

    private static UserProfile CreateProfileEnrolledIn(Guid userId, Course course)
    {
        var profile = CreateProfile(userId);
        profile.EnrollInCourse(course);
        return profile;
    }

    private static Course CreateCourseWithId(Guid courseId)
    {
        var subChapter = CreateSubChapter();
        var chapter = Chapter.Create(1, "Intro", "Introduction concept", [subChapter]);
        var course = Course.Create("Test Topic", [chapter]);
        SetEntityId(course, courseId);
        return course;
    }

    /// <summary>
    ///     Instantiates SubChapter via reflection since it has a private constructor.
    ///     Adjust the field assignments if SubChapter's internals change.
    /// </summary>
    private static SubChapter CreateSubChapter()
    {
        var subChapter = (SubChapter)RuntimeHelpers
            .GetUninitializedObject(typeof(SubChapter));

        SetPrivateField(subChapter, "_title", "Intro SubChapter");
        SetPrivateField(subChapter, "_contentSummary", "Summary");

        return subChapter;
    }

    private static ChapterExpandResponse CreateChapterExpandResponse()
    {
        return new ChapterExpandResponse(
            new ChapterDto("Day 1", "Intro", 1),
            new ExpandedContentDto([]), // adjust if ExpandedContentDto signature differs
            [], // List<SubchapterContentDto>
            [] // List<QuestionDto>
        );
    }

    private static CourseSkeletonResponse EmptySkeleton()
    {
        return new CourseSkeletonResponse(new SkeletonDto("topic", []), 1);
    }

    private static CourseSkeletonResponse SkeletonWithChapters(params ChapterDto[] chapters)
    {
        return new CourseSkeletonResponse(new SkeletonDto("Python", [.. chapters]), chapters.Length);
    }

    /// <summary>
    ///     Sets the Id on an Entity base class via reflection since entity IDs
    ///     are normally assigned by EF — not exposed via constructor.
    /// </summary>
    private static void SetEntityId(Entity entity, Guid id)
    {
        var prop = typeof(Entity).GetProperty("Id")
                   ?? throw new InvalidOperationException("Entity does not expose an Id property.");
        prop.SetValue(entity, id);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType()
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(target, value);
    }

    // -------------------------------------------------------------------------
    // UploadManyAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UploadManyAsync_ValidFile_UploadsAndEnqueuesJob()
    {
        var userId = Guid.NewGuid();
        _storage.Setup(s => s.UploadAsync(
                It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("storage-key");

        var result = await _sut.UploadManyAsync(userId, [CreateFile()], "My Course");

        result.TotalFiles.ShouldBe(1);
        result.Accepted.ShouldBe(1);
        result.Rejected.ShouldBe(0);
        result.Errors.ShouldBeEmpty();
        _courseUploadsRepository.Verify(r =>
            r.AddRangeAsync(It.IsAny<List<CourseUpload>>(), It.IsAny<CancellationToken>()), Times.Once);
        _courseJobRepository.Verify(r =>
            r.AddRangeAsync(It.IsAny<List<CourseJob>>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _jobClient.Verify(j =>
                j.Create(It.IsAny<Job>(), It.IsAny<IState>()),
            Times.Once);
    }

    [Fact]
    public async Task UploadManyAsync_MultipleValidFiles_EnqueuesOneJobPerFile()
    {
        var files = Enumerable.Range(0, 3).Select(i => CreateFile($"doc{i}.pdf")).ToList();
        _storage.Setup(s => s.UploadAsync(
                It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("key");

        var result = await _sut.UploadManyAsync(Guid.NewGuid(), files, "Course");

        result.Accepted.ShouldBe(3);
        _jobClient.Verify(j =>
                j.Create(It.IsAny<Job>(), It.IsAny<IState>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task UploadManyAsync_AllFilesInvalid_ReturnsEarlyWithoutPersisting()
    {
        var result = await _sut.UploadManyAsync(Guid.NewGuid(), [EmptyFile()], "Course");

        result.Accepted.ShouldBe(0);
        result.Rejected.ShouldBe(1);
        result.Errors.ShouldNotBeEmpty();
        _courseUploadsRepository.Verify(r =>
            r.AddRangeAsync(It.IsAny<List<CourseUpload>>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _jobClient.Verify(j =>
                j.Create(It.IsAny<Job>(), It.IsAny<IState>()),
            Times.Never);
    }

    [Fact]
    public async Task UploadManyAsync_MixedFiles_OnlyUploadsValidOnes()
    {
        var files = new List<UploadFileDto> { CreateFile("good.pdf"), EmptyFile("bad.pdf") };
        _storage.Setup(s => s.UploadAsync(
                It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("key");

        var result = await _sut.UploadManyAsync(Guid.NewGuid(), files, "Course");

        result.TotalFiles.ShouldBe(2);
        result.Accepted.ShouldBe(1);
        result.Rejected.ShouldBe(1);
        result.Errors.Count.ShouldBe(1);
        result.Errors[0].ShouldContain("bad.pdf");
    }

    [Fact]
    public async Task UploadManyAsync_EmptyFileContent_AddsError()
    {
        var result = await _sut.UploadManyAsync(Guid.NewGuid(), [EmptyFile()], "Course");

        result.Errors.ShouldContain(e => e.Contains("empty.pdf") && e.Contains("empty"));
    }

    [Fact]
    public async Task UploadManyAsync_OversizedFile_AddsError()
    {
        var result = await _sut.UploadManyAsync(Guid.NewGuid(), [OversizedFile()], "Course");

        result.Errors.ShouldContain(e => e.Contains("big.pdf") && e.Contains("20MB"));
    }

    [Fact]
    public async Task UploadManyAsync_DisallowedExtension_AddsError()
    {
        var result = await _sut.UploadManyAsync(Guid.NewGuid(), [DisallowedFile()], "Course");

        result.Errors.ShouldContain(e => e.Contains("virus.exe"));
    }

    [Fact]
    public async Task UploadManyAsync_ExceedsMaxFileCount_AddsErrorForExtraFiles()
    {
        var files = Enumerable.Range(0, CourseConstants.MaxFiles + 2)
            .Select(i => CreateFile($"doc{i}.pdf"))
            .ToList();
        _storage.Setup(s => s.UploadAsync(
                It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("key");

        var result = await _sut.UploadManyAsync(Guid.NewGuid(), files, "Course");

        result.Accepted.ShouldBe(CourseConstants.MaxFiles);
        result.Rejected.ShouldBe(2);
        result.Errors.ShouldAllBe(e => e.Contains($"max {CourseConstants.MaxFiles}"));
    }

    [Fact]
    public async Task UploadManyAsync_ExactlyMaxFileCount_AllSucceed()
    {
        var files = Enumerable.Range(0, CourseConstants.MaxFiles)
            .Select(i => CreateFile($"doc{i}.pdf"))
            .ToList();
        _storage.Setup(s => s.UploadAsync(
                It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("key");

        var result = await _sut.UploadManyAsync(Guid.NewGuid(), files, "Course");

        result.Accepted.ShouldBe(CourseConstants.MaxFiles);
        result.Errors.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("doc.pdf")]
    [InlineData("doc.doc")]
    [InlineData("doc.docx")]
    public async Task UploadManyAsync_AllowedExtensions_PassValidation(string fileName)
    {
        _storage.Setup(s => s.UploadAsync(
                It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("key");

        var result = await _sut.UploadManyAsync(Guid.NewGuid(), [CreateFile(fileName)], "Course");

        result.Accepted.ShouldBe(1);
        result.Errors.ShouldBeEmpty();
    }

    // -------------------------------------------------------------------------
    // FinishChapterAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task FinishChapterAsync_ValidRequest_FinishesChapterAndSaves()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var chapterId = Guid.NewGuid();
        var course = CreateCourseWithId(courseId);
        var profile = CreateProfileEnrolledIn(userId, course);

        _userProfileRepository.Setup(r => r.GetCurrentUserProfile(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _chapterRepository.Setup(r => r.ExistsAsync(chapterId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _sut.FinishChapterAsync(userId, courseId, chapterId);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task FinishChapterAsync_UserProfileNotFound_ThrowsUserProfileNotFoundException()
    {
        _userProfileRepository.Setup(r => r.GetCurrentUserProfile(
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile?)null);

        await Should.ThrowAsync<UserProfileNotFoundException>(() =>
            _sut.FinishChapterAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task FinishChapterAsync_ChapterNotFound_ThrowsChapterNotFoundException()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var course = CreateCourseWithId(courseId);
        var profile = CreateProfileEnrolledIn(userId, course);

        _userProfileRepository.Setup(r => r.GetCurrentUserProfile(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _chapterRepository.Setup(r => r.ExistsAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Should.ThrowAsync<ChapterNotFoundException>(() =>
            _sut.FinishChapterAsync(userId, courseId, Guid.NewGuid()));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task FinishChapterAsync_UserNotEnrolledInCourse_ThrowsInvalidFieldException()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var profile = CreateProfile(userId); // enrolled in nothing

        _userProfileRepository.Setup(r => r.GetCurrentUserProfile(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _chapterRepository.Setup(r => r.ExistsAsync(
                It.IsAny<Guid>(), courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Should.ThrowAsync<InvalidFieldException>(() => _sut.FinishChapterAsync(userId, courseId, Guid.NewGuid()));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // -------------------------------------------------------------------------
    // GetCourseSkeletonAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetCourseSkeletonAsync_CacheHit_ReturnsCachedValueWithoutCallingAnalyzer()
    {
        var request = new CourseSkeletonRequest("Python");
        var cached = EmptySkeleton();
        _cacheService.Setup(c => c.GetAsync<CourseSkeletonResponse>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var result = await _sut.GetCourseSkeletonAsync(request);

        result.ShouldBe(cached);
        _analyzer.Verify(a => a.GetCourseSkeletonAsync(
            It.IsAny<CourseSkeletonRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _cacheService.Verify(c => c.SetAsync(
            It.IsAny<string>(), It.IsAny<CourseSkeletonResponse>(),
            It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetCourseSkeletonAsync_CacheMiss_CallsAnalyzerAndCachesResult()
    {
        var request = new CourseSkeletonRequest("Python");
        var skeleton = EmptySkeleton();
        _cacheService.Setup(c => c.GetAsync<CourseSkeletonResponse>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CourseSkeletonResponse?)null);
        _analyzer.Setup(a => a.GetCourseSkeletonAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(skeleton);

        var result = await _sut.GetCourseSkeletonAsync(request);

        result.ShouldBe(skeleton);
        _analyzer.Verify(a => a.GetCourseSkeletonAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _cacheService.Verify(c => c.SetAsync(
            It.Is<string>(k => k.Contains("Python")),
            skeleton,
            TimeSpan.FromHours(2),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // -------------------------------------------------------------------------
    // SaveCourseContentAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SaveCourseContentAsync_CacheHit_PersistsAndReturnsCourseId()
    {
        var userId = Guid.NewGuid();
        var expectedId = Guid.NewGuid();
        _cacheService.Setup(c => c.GetAsync<List<ChapterExpandResponse>>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _coursePersistenceService.Setup(p => p.PersistAsync(
                userId, "Python", It.IsAny<List<ChapterAssemblyModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        var result = await _sut.SaveCourseContentAsync(userId, "Python", EmptySkeleton());

        result.ShouldBe(expectedId);
        _analyzer.Verify(a => a.GetExpandedChapterAsync(
            It.IsAny<ChapterRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _coursePersistenceService.Verify(p => p.PersistAsync(
            userId, "Python", It.IsAny<List<ChapterAssemblyModel>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveCourseContentAsync_CacheMiss_ExpandsChaptersAndCachesResult()
    {
        var userId = Guid.NewGuid();
        var response = SkeletonWithChapters(new ChapterDto("Day 1", "Intro", 1));

        _cacheService.Setup(c => c.GetAsync<List<ChapterExpandResponse>>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<ChapterExpandResponse>?)null);
        _analyzer.Setup(a => a.GetExpandedChapterAsync(
                It.IsAny<ChapterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateChapterExpandResponse());
        _coursePersistenceService.Setup(p => p.PersistAsync(
                It.IsAny<Guid>(), It.IsAny<string>(),
                It.IsAny<List<ChapterAssemblyModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        await _sut.SaveCourseContentAsync(userId, "Python", response);

        _analyzer.Verify(a => a.GetExpandedChapterAsync(
            It.IsAny<ChapterRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _cacheService.Verify(c => c.SetAsync(
            It.Is<string>(k => k.Contains("Python")),
            It.IsAny<List<ChapterExpandResponse>>(),
            TimeSpan.FromHours(2),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}