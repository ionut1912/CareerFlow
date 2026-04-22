using System.Net;
using System.Text;
using System.Text.Json;
using CareerFlow.Core.Domain.Models.AI.Dto;
using CareerFlow.Core.Domain.Models.AI.Requests;
using CareerFlow.Core.Domain.Models.AI.Responses;
using CareerFlow.Core.Infrastructure.Services;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Unit.Services;

public class CourseGenerationServiceTests:IDisposable
{
    private static readonly JsonSerializerOptions SnakeCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    private readonly FakeHttpMessageHandler _handler = new();
    private readonly CourseGenerationService _sut;

    public CourseGenerationServiceTests()
    {
        var client = new HttpClient(_handler) { BaseAddress = new Uri("https://api.test.com") };
        _sut = new CourseGenerationService(client);
    }

    private static StringContent JsonContent<T>(T value)
    {
        return new StringContent(JsonSerializer.Serialize(value, SnakeCaseOptions),
            Encoding.UTF8,
            "application/json");
    }

    private static CourseSkeletonResponse BuildSkeletonResponse()
    {
        return new CourseSkeletonResponse(
            new SkeletonDto(
                "C# Basics",
                [
                    new ChapterDto("Intro", "Overview", 1),
                    new ChapterDto("OOP", "Classes", 2)
                ]),
            2);
    }

    private static ChapterExpandResponse BuildChapterExpandResponse()
    {
        return new ChapterExpandResponse(
            new ChapterDto("OOP", "Classes", 2),
            new ExpandedContentDto(
            [
                new SubchapterDto("Classes", "How to define classes")
            ]),
            [
                new SubchapterContentDto(
                    "<p>Theory</p>",
                    [
                        new QuestionDto(
                            "What is a class?",
                            [
                                new QuestionOptionDto("A blueprint", true),
                                new QuestionOptionDto("A variable", false)
                            ])
                    ])
            ],
            [
                new QuestionDto(
                    "What is OOP?",
                    [
                        new QuestionOptionDto("A paradigm", true),
                        new QuestionOptionDto("A language", false)
                    ])
            ]);
    }

    [Fact]
    public void Constructor_ShouldSetTimeoutToTenMinutes()
    {
        var client = new HttpClient(_handler) { BaseAddress = new Uri("https://api.test.com") };
        _=new CourseGenerationService(client);
        client.Timeout.ShouldBe(TimeSpan.FromMinutes(10));
    }

    [Fact]
    public async Task GetCourseSkeletonAsync_ShouldPostToCorrectEndpoint()
    {
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildSkeletonResponse()));

        await _sut.GetCourseSkeletonAsync(new CourseSkeletonRequest("C# Basics"), CancellationToken.None);

        _handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        _handler.LastRequest.RequestUri!.PathAndQuery.ShouldBe("/courses/skeleton");
    }

    [Fact]
    public async Task GetCourseSkeletonAsync_ShouldSerializeRequestWithSnakeCase()
    {
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildSkeletonResponse()));

        await _sut.GetCourseSkeletonAsync(new CourseSkeletonRequest("C# Basics"), CancellationToken.None);

        string body = await _handler.LastRequest!.Content!.ReadAsStringAsync();
        body.ShouldContain("\"topic\"");
        body.ShouldContain("C# Basics");
    }

    [Fact]
    public async Task GetCourseSkeletonAsync_WhenSuccessResponse_ShouldReturnDeserializedSkeleton()
    {
        CourseSkeletonResponse expected = BuildSkeletonResponse();
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(expected));

        CourseSkeletonResponse result = await _sut.GetCourseSkeletonAsync(new CourseSkeletonRequest("C# Basics"), CancellationToken.None);

        result.ShouldNotBeNull();
        result.EstimatedDays.ShouldBe(expected.EstimatedDays);
        result.Skeleton.Topic.ShouldBe(expected.Skeleton.Topic);
        result.Skeleton.Chapters.Count.ShouldBe(expected.Skeleton.Chapters.Count);
    }

    [Fact]
    public async Task GetCourseSkeletonAsync_WhenSuccessResponse_ShouldDeserializeChaptersCorrectly()
    {
        CourseSkeletonResponse expected = BuildSkeletonResponse();
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(expected));

        CourseSkeletonResponse result = await _sut.GetCourseSkeletonAsync(new CourseSkeletonRequest("C# Basics"), CancellationToken.None);

        ChapterDto first = result.Skeleton.Chapters[0];
        first.Title.ShouldBe("Intro");
        first.CoreConcept.ShouldBe("Overview");
        first.Day.ShouldBe(1);
    }

    [Fact]
    public async Task GetCourseSkeletonAsync_WhenNonSuccessResponse_ShouldThrowHttpRequestException()
    {
        _handler.RespondWith(HttpStatusCode.InternalServerError);

        await Should.ThrowAsync<HttpRequestException>(() =>
            _sut.GetCourseSkeletonAsync(new CourseSkeletonRequest("C# Basics"), CancellationToken.None));
    }

    [Fact]
    public async Task GetCourseSkeletonAsync_WhenUnauthorized_ShouldThrowHttpRequestException()
    {
        _handler.RespondWith(HttpStatusCode.Unauthorized);

        await Should.ThrowAsync<HttpRequestException>(() =>
            _sut.GetCourseSkeletonAsync(new CourseSkeletonRequest("C# Basics"), CancellationToken.None));
    }

    [Fact]
    public async Task GetCourseSkeletonAsync_WhenNullJsonResponse_ShouldThrowInvalidOperationException()
    {
        _handler.RespondWith(HttpStatusCode.OK, JsonContent<CourseSkeletonResponse?>(null));

        await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.GetCourseSkeletonAsync(new CourseSkeletonRequest("C# Basics"), CancellationToken.None));
    }

    [Fact]
    public async Task GetExpandedChapterAsync_ShouldPostToCorrectEndpoint()
    {
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildChapterExpandResponse()));

        await _sut.GetExpandedChapterAsync(
            new ChapterRequest("C# Basics", "OOP", "Classes"),
            CancellationToken.None);

        _handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        _handler.LastRequest.RequestUri!.PathAndQuery.ShouldBe("/courses//chapters/expand");
    }

    [Fact]
    public async Task GetExpandedChapterAsync_ShouldSerializeRequestWithSnakeCase()
    {
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildChapterExpandResponse()));

        await _sut.GetExpandedChapterAsync(
            new ChapterRequest("C# Basics", "OOP", "Classes"),
            CancellationToken.None);

        string body = await _handler.LastRequest!.Content!.ReadAsStringAsync();
        body.ShouldContain("\"topic\"");
        body.ShouldContain("\"chapter_title\"");
        body.ShouldContain("\"core_concept\"");
        body.ShouldContain("C# Basics");
        body.ShouldContain("OOP");
        body.ShouldContain("Classes");
    }

    [Fact]
    public async Task GetExpandedChapterAsync_WhenSuccessResponse_ShouldReturnDeserializedChapter()
    {
        ChapterExpandResponse expected = BuildChapterExpandResponse();
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(expected));

        ChapterExpandResponse result = await _sut.GetExpandedChapterAsync(
            new ChapterRequest("C# Basics", "OOP", "Classes"),
            CancellationToken.None);

        result.ShouldNotBeNull();
        result.Chapter.Title.ShouldBe(expected.Chapter.Title);
        result.Chapter.CoreConcept.ShouldBe(expected.Chapter.CoreConcept);
        result.Chapter.Day.ShouldBe(expected.Chapter.Day);
    }

    [Fact]
    public async Task GetExpandedChapterAsync_WhenSuccessResponse_ShouldDeserializeExpandedContentCorrectly()
    {
        ChapterExpandResponse expected = BuildChapterExpandResponse();
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(expected));

        ChapterExpandResponse result = await _sut.GetExpandedChapterAsync(
            new ChapterRequest("C# Basics", "OOP", "Classes"),
            CancellationToken.None);

        result.Expanded.Subchapters.Count.ShouldBe(1);
        result.Expanded.Subchapters[0].Title.ShouldBe("Classes");
        result.Expanded.Subchapters[0].ContentSummary.ShouldBe("How to define classes");
    }

    [Fact]
    public async Task GetExpandedChapterAsync_WhenSuccessResponse_ShouldDeserializeSubchapterContentsCorrectly()
    {
        ChapterExpandResponse expected = BuildChapterExpandResponse();
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(expected));

        ChapterExpandResponse result = await _sut.GetExpandedChapterAsync(
            new ChapterRequest("C# Basics", "OOP", "Classes"),
            CancellationToken.None);

        result.SubchapterContents.Count.ShouldBe(1);
        result.SubchapterContents[0].TheoryHtml.ShouldBe("<p>Theory</p>");
        result.SubchapterContents[0].Quiz.Count.ShouldBe(1);
        result.SubchapterContents[0].Quiz[0].Question.ShouldBe("What is a class?");
    }

    [Fact]
    public async Task GetExpandedChapterAsync_WhenSuccessResponse_ShouldDeserializeFinalQuizCorrectly()
    {
        ChapterExpandResponse expected = BuildChapterExpandResponse();
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(expected));

        ChapterExpandResponse result = await _sut.GetExpandedChapterAsync(
            new ChapterRequest("C# Basics", "OOP", "Classes"),
            CancellationToken.None);

        result.FinalQuiz.Count.ShouldBe(1);
        result.FinalQuiz[0].Question.ShouldBe("What is OOP?");
        result.FinalQuiz[0].Options.Count.ShouldBe(2);
        result.FinalQuiz[0].Options.Single(o => o.IsCorrect).Label.ShouldBe("A paradigm");
    }

    [Fact]
    public async Task GetExpandedChapterAsync_WhenNonSuccessResponse_ShouldThrowHttpRequestException()
    {
        _handler.RespondWith(HttpStatusCode.InternalServerError);

        await Should.ThrowAsync<HttpRequestException>(() =>
            _sut.GetExpandedChapterAsync(
                new ChapterRequest("C# Basics", "OOP", "Classes"),
                CancellationToken.None));
    }

    [Fact]
    public async Task GetExpandedChapterAsync_WhenUnauthorized_ShouldThrowHttpRequestException()
    {
        _handler.RespondWith(HttpStatusCode.Unauthorized);

        await Should.ThrowAsync<HttpRequestException>(() =>
            _sut.GetExpandedChapterAsync(
                new ChapterRequest("C# Basics", "OOP", "Classes"),
                CancellationToken.None));
    }

    [Fact]
    public async Task GetExpandedChapterAsync_WhenNullJsonResponse_ShouldThrowInvalidOperationException()
    {
        _handler.RespondWith(HttpStatusCode.OK, JsonContent<ChapterExpandResponse?>(null));

        await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.GetExpandedChapterAsync(
                new ChapterRequest("C# Basics", "OOP", "Classes"),
                CancellationToken.None));
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private HttpContent? _content;
        private HttpStatusCode _statusCode = HttpStatusCode.OK;

        public HttpRequestMessage? LastRequest { get; private set; }

        public void RespondWith(HttpStatusCode statusCode, HttpContent? content = null)
        {
            _statusCode = statusCode;
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(_statusCode) { Content = _content });
        }
    }

    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }
}
