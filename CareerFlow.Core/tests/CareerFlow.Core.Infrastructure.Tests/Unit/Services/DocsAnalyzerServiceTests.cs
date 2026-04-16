using System.Net;
using System.Text;
using System.Text.Json;
using CareerFlow.Core.Domain.Models.AI.Dto;
using CareerFlow.Core.Domain.Models.AI.Requests;
using CareerFlow.Core.Domain.Models.AI.Responses;
using CareerFlow.Core.Domain.Models.Course.Dto;
using CareerFlow.Core.Infrastructure.Services;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Unit.Services;

public class DocsAnalyzerServiceTests
{
    private static readonly JsonSerializerOptions SnakeCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    private readonly FakeHttpMessageHandler _handler = new();
    private readonly DocsAnalyzerService _sut;

    public DocsAnalyzerServiceTests()
    {
        var client = new HttpClient(_handler) { BaseAddress = new Uri("https://api.test.com") };
        _sut = new DocsAnalyzerService(client);
    }

    private static StringContent JsonContent<T>(T value)
    {
        return new StringContent(JsonSerializer.Serialize(value, SnakeCaseOptions),
            Encoding.UTF8,
            "application/json");
    }

    private static UploadFileDto BuildUploadFileDto()
    {
        return new UploadFileDto(
            "test.pdf",
            "application/pdf",
            new MemoryStream("file content"u8.ToArray()));
    }

    private static DocumentProcessingResponse BuildDocumentProcessingResponse()
    {
        return new DocumentProcessingResponse(
            "doc-123",
            new DocumentAnalysisDto(
                "C# Guide",
                "A comprehensive guide",
                ["OOP", "Async", "LINQ"]),
            new SkeletonDto(
                "C# Programming",
                [
                    new ChapterDto("Intro", "Overview", 1),
                    new ChapterDto("OOP", "Classes", 2)
                ]),
            5);
    }

    private static ChapterDetailResponse BuildChapterDetailResponse()
    {
        return new ChapterDetailResponse(
            [
                new DetailedSubchapterDto(
                    "Classes",
                    "How to define classes",
                    "<p>A class is a blueprint</p>",
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
        new DocsAnalyzerService(client);
        client.Timeout.ShouldBe(TimeSpan.FromMinutes(10));
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_ShouldPostToCorrectEndpoint()
    {
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildDocumentProcessingResponse()));

        await _sut.AnalyzeDocumentAsync(BuildUploadFileDto(), CancellationToken.None);

        _handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        _handler.LastRequest.RequestUri!.PathAndQuery.ShouldBe("/document-courses/upload-and-analyze");
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_ShouldSendMultipartFormDataContent()
    {
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildDocumentProcessingResponse()));

        await _sut.AnalyzeDocumentAsync(BuildUploadFileDto(), CancellationToken.None);

        _handler.MultipartParts.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_ShouldIncludeFileInMultipartWithCorrectName()
    {
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildDocumentProcessingResponse()));

        await _sut.AnalyzeDocumentAsync(BuildUploadFileDto(), CancellationToken.None);

        var filePart = _handler.MultipartParts.FirstOrDefault(p => p.Name == "file");
        filePart.Name.ShouldNotBeNull();
        filePart.FileName.ShouldBe("test.pdf");
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_ShouldSetCorrectContentTypeOnFilePart()
    {
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildDocumentProcessingResponse()));

        await _sut.AnalyzeDocumentAsync(BuildUploadFileDto(), CancellationToken.None);

        var filePart = _handler.MultipartParts.First(p => p.Name == "file");
        filePart.ContentType.ShouldBe("application/pdf");
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_WhenContentTypeIsNull_ShouldFallBackToOctetStream()
    {
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildDocumentProcessingResponse()));

        var dto = new UploadFileDto(
            "test.bin",
            null!,
            new MemoryStream("data"u8.ToArray()));

        await _sut.AnalyzeDocumentAsync(dto, CancellationToken.None);

        var filePart = _handler.MultipartParts.First(p => p.Name == "file");
        filePart.ContentType.ShouldBe("application/octet-stream");
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_WhenSuccessResponse_ShouldReturnDeserializedDocumentId()
    {
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildDocumentProcessingResponse()));

        var result = await _sut.AnalyzeDocumentAsync(BuildUploadFileDto(), CancellationToken.None);

        result.DocumentId.ShouldBe("doc-123");
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_WhenSuccessResponse_ShouldReturnDeserializedAnalysis()
    {
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildDocumentProcessingResponse()));

        var result = await _sut.AnalyzeDocumentAsync(BuildUploadFileDto(), CancellationToken.None);

        result.Analysis.Title.ShouldBe("C# Guide");
        result.Analysis.Summary.ShouldBe("A comprehensive guide");
        result.Analysis.KeyTopics.ShouldBe(["OOP", "Async", "LINQ"]);
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_WhenSuccessResponse_ShouldReturnDeserializedSkeleton()
    {
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildDocumentProcessingResponse()));

        var result = await _sut.AnalyzeDocumentAsync(BuildUploadFileDto(), CancellationToken.None);

        result.Skeleton.Topic.ShouldBe("C# Programming");
        result.Skeleton.Chapters.Count.ShouldBe(2);
        result.Skeleton.Chapters[0].Title.ShouldBe("Intro");
        result.Skeleton.Chapters[1].Title.ShouldBe("OOP");
        result.EstimatedDays.ShouldBe(5);
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_WhenNonSuccessResponse_ShouldThrowHttpRequestException()
    {
        _handler.RespondWith(HttpStatusCode.InternalServerError);

        await Should.ThrowAsync<HttpRequestException>(() =>
            _sut.AnalyzeDocumentAsync(BuildUploadFileDto(), CancellationToken.None));
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_WhenUnauthorized_ShouldThrowHttpRequestException()
    {
        _handler.RespondWith(HttpStatusCode.Unauthorized);

        await Should.ThrowAsync<HttpRequestException>(() =>
            _sut.AnalyzeDocumentAsync(BuildUploadFileDto(), CancellationToken.None));
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_WhenNullJsonResponse_ShouldThrowInvalidOperationException()
    {
        _handler.RespondWith(HttpStatusCode.OK, JsonContent<DocumentProcessingResponse?>(null));

        await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.AnalyzeDocumentAsync(BuildUploadFileDto(), CancellationToken.None));
    }

    [Fact]
    public async Task ExpandAnalyzedDocument_ShouldPostToCorrectEndpoint()
    {
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildChapterDetailResponse()));

        await _sut.ExpandAnalyzedDocument(
            new DocumentChapterRequest("OOP", "Classes", "doc-123"),
            CancellationToken.None);

        _handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        _handler.LastRequest.RequestUri!.PathAndQuery.ShouldBe("/document-courses/chapters/expand");
    }

    [Fact]
    public async Task ExpandAnalyzedDocument_ShouldSerializeRequestWithSnakeCase()
    {
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildChapterDetailResponse()));

        await _sut.ExpandAnalyzedDocument(
            new DocumentChapterRequest("OOP", "Classes", "doc-123"),
            CancellationToken.None);

        _handler.LastRequestBody.ShouldContain("\"chapter_title\"");
        _handler.LastRequestBody.ShouldContain("\"core_concept\"");
        _handler.LastRequestBody.ShouldContain("\"document_id\"");
        _handler.LastRequestBody.ShouldNotContain("ChapterTitle");
        _handler.LastRequestBody.ShouldNotContain("CoreConcept");
        _handler.LastRequestBody.ShouldNotContain("DocumentId");
    }

    [Fact]
    public async Task ExpandAnalyzedDocument_WhenSuccessResponse_ShouldReturnDeserializedSubchapters()
    {
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildChapterDetailResponse()));

        var result = await _sut.ExpandAnalyzedDocument(
            new DocumentChapterRequest("OOP", "Classes", "doc-123"),
            CancellationToken.None);

        result.Subchapters.Count.ShouldBe(1);
        result.Subchapters[0].Title.ShouldBe("Classes");
        result.Subchapters[0].ContentSummary.ShouldBe("How to define classes");
        result.Subchapters[0].TheoryHtml.ShouldBe("<p>A class is a blueprint</p>");
    }

    [Fact]
    public async Task ExpandAnalyzedDocument_WhenSuccessResponse_ShouldReturnDeserializedSubchapterQuiz()
    {
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildChapterDetailResponse()));

        var result = await _sut.ExpandAnalyzedDocument(
            new DocumentChapterRequest("OOP", "Classes", "doc-123"),
            CancellationToken.None);

        var quiz = result.Subchapters[0].Quiz;
        quiz.Count.ShouldBe(1);
        quiz[0].Question.ShouldBe("What is a class?");
        quiz[0].Options.Single(o => o.IsCorrect).Label.ShouldBe("A blueprint");
    }

    [Fact]
    public async Task ExpandAnalyzedDocument_WhenSuccessResponse_ShouldReturnDeserializedRecapQuiz()
    {
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildChapterDetailResponse()));

        var result = await _sut.ExpandAnalyzedDocument(
            new DocumentChapterRequest("OOP", "Classes", "doc-123"),
            CancellationToken.None);

        result.RecapQuiz.Count.ShouldBe(1);
        result.RecapQuiz[0].Question.ShouldBe("What is OOP?");
        result.RecapQuiz[0].Options.Single(o => o.IsCorrect).Label.ShouldBe("A paradigm");
    }

    [Fact]
    public async Task ExpandAnalyzedDocument_WhenNonSuccessResponse_ShouldThrowHttpRequestException()
    {
        _handler.RespondWith(HttpStatusCode.InternalServerError);

        await Should.ThrowAsync<HttpRequestException>(() =>
            _sut.ExpandAnalyzedDocument(
                new DocumentChapterRequest("OOP", "Classes", "doc-123"),
                CancellationToken.None));
    }

    [Fact]
    public async Task ExpandAnalyzedDocument_WhenUnauthorized_ShouldThrowHttpRequestException()
    {
        _handler.RespondWith(HttpStatusCode.Unauthorized);

        await Should.ThrowAsync<HttpRequestException>(() =>
            _sut.ExpandAnalyzedDocument(
                new DocumentChapterRequest("OOP", "Classes", "doc-123"),
                CancellationToken.None));
    }

    [Fact]
    public async Task ExpandAnalyzedDocument_WhenNullJsonResponse_ShouldThrowInvalidOperationException()
    {
        _handler.RespondWith(HttpStatusCode.OK, JsonContent<ChapterDetailResponse?>(null));

        await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.ExpandAnalyzedDocument(
                new DocumentChapterRequest("OOP", "Classes", "doc-123"),
                CancellationToken.None));
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private HttpContent? _responseContent;
        private HttpStatusCode _statusCode = HttpStatusCode.OK;

        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastRequestBody { get; private set; }
        public List<CapturedPart> MultipartParts { get; private set; } = [];

        public void RespondWith(HttpStatusCode statusCode, HttpContent? content = null)
        {
            _statusCode = statusCode;
            _responseContent = content;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;

            if (request.Content is MultipartFormDataContent multipart)
            {
                MultipartParts = [];
                foreach (var part in multipart)
                    MultipartParts.Add(new CapturedPart(
                        part.Headers.ContentDisposition?.Name?.Trim('"'),
                        part.Headers.ContentDisposition?.FileName?.Trim('"'),
                        part.Headers.ContentType?.MediaType,
                        await part.ReadAsByteArrayAsync(cancellationToken)));
            }
            else if (request.Content is not null)
            {
                LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            return new HttpResponseMessage(_statusCode) { Content = _responseContent };
        }
    }

    private sealed record CapturedPart(
        string? Name,
        string? FileName,
        string? ContentType,
        byte[] Data);
}