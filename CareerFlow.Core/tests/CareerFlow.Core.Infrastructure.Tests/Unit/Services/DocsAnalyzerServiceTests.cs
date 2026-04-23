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

public class DocsAnalyzerServiceTests : IDisposable
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

    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }

    // ── Constructor ──────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_ShouldSetTimeoutToTenMinutes()
    {
        // Arrange
        var client = new HttpClient(_handler) { BaseAddress = new Uri("https://api.test.com") };

        // Act
        _ = new DocsAnalyzerService(client);

        // Assert
        client.Timeout.ShouldBe(TimeSpan.FromMinutes(10));
    }

    // ── AnalyzeDocumentAsync — request shape ─────────────────────────────────

    [Fact]
    public async Task AnalyzeDocumentAsync_ShouldPostToCorrectEndpoint()
    {
        // Arrange
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildDocumentProcessingResponse()));

        // Act
        await _sut.AnalyzeDocumentAsync(BuildUploadFileDto(), CancellationToken.None);

        // Assert
        _handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        _handler.LastRequest.RequestUri!.PathAndQuery.ShouldBe("/document-courses/upload-and-analyze");
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_ShouldSendMultipartFormDataContent()
    {
        // Arrange
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildDocumentProcessingResponse()));

        // Act
        await _sut.AnalyzeDocumentAsync(BuildUploadFileDto(), CancellationToken.None);

        // Assert
        _handler.MultipartParts.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_ShouldIncludeFileInMultipartWithCorrectName()
    {
        // Arrange
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildDocumentProcessingResponse()));

        // Act
        await _sut.AnalyzeDocumentAsync(BuildUploadFileDto(), CancellationToken.None);

        // Assert
        CapturedPart? filePart = _handler.MultipartParts.FirstOrDefault(p => p.Name == "file");
        filePart.ShouldNotBeNull();
        filePart.FileName.ShouldBe("test.pdf");
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_ShouldSetCorrectContentTypeOnFilePart()
    {
        // Arrange
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildDocumentProcessingResponse()));

        // Act
        await _sut.AnalyzeDocumentAsync(BuildUploadFileDto(), CancellationToken.None);

        // Assert
        CapturedPart filePart = _handler.MultipartParts.First(p => p.Name == "file");
        filePart.ContentType.ShouldBe("application/pdf");
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_WhenContentTypeIsNull_ShouldFallBackToOctetStream()
    {
        // Arrange
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildDocumentProcessingResponse()));
        var dto = new UploadFileDto("test.bin", null!, new MemoryStream("data"u8.ToArray()));

        // Act
        await _sut.AnalyzeDocumentAsync(dto, CancellationToken.None);

        // Assert
        CapturedPart filePart = _handler.MultipartParts.First(p => p.Name == "file");
        filePart.ContentType.ShouldBe("application/octet-stream");
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_WhenContentTypeIsWhiteSpace_ShouldFallBackToOctetStream()
    {
        // Arrange
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildDocumentProcessingResponse()));
        var dto = new UploadFileDto("test.bin", "   ", new MemoryStream("data"u8.ToArray()));

        // Act
        await _sut.AnalyzeDocumentAsync(dto, CancellationToken.None);

        // Assert
        CapturedPart filePart = _handler.MultipartParts.First(p => p.Name == "file");
        filePart.ContentType.ShouldBe("application/octet-stream");
    }

    // ── AnalyzeDocumentAsync — response deserialization ──────────────────────

    [Fact]
    public async Task AnalyzeDocumentAsync_WhenSuccessResponse_ShouldReturnDeserializedDocumentId()
    {
        // Arrange
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildDocumentProcessingResponse()));

        // Act
        DocumentProcessingResponse result =
            await _sut.AnalyzeDocumentAsync(BuildUploadFileDto(), CancellationToken.None);

        // Assert
        result.DocumentId.ShouldBe("doc-123");
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_WhenSuccessResponse_ShouldReturnDeserializedAnalysis()
    {
        // Arrange
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildDocumentProcessingResponse()));

        // Act
        DocumentProcessingResponse result =
            await _sut.AnalyzeDocumentAsync(BuildUploadFileDto(), CancellationToken.None);

        // Assert
        result.Analysis.Title.ShouldBe("C# Guide");
        result.Analysis.Summary.ShouldBe("A comprehensive guide");
        result.Analysis.KeyTopics.ShouldBe(["OOP", "Async", "LINQ"]);
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_WhenSuccessResponse_ShouldReturnDeserializedSkeleton()
    {
        // Arrange
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildDocumentProcessingResponse()));

        // Act
        DocumentProcessingResponse result =
            await _sut.AnalyzeDocumentAsync(BuildUploadFileDto(), CancellationToken.None);

        // Assert
        result.Skeleton.Topic.ShouldBe("C# Programming");
        result.Skeleton.Chapters.Count.ShouldBe(2);
        result.Skeleton.Chapters[0].Title.ShouldBe("Intro");
        result.Skeleton.Chapters[1].Title.ShouldBe("OOP");
        result.EstimatedDays.ShouldBe(5);
    }

    // ── AnalyzeDocumentAsync — error handling ────────────────────────────────

    [Fact]
    public async Task AnalyzeDocumentAsync_WhenNonSuccessResponse_ShouldThrowHttpRequestException()
    {
        // Arrange
        _handler.RespondWith(HttpStatusCode.InternalServerError);

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(() =>
            _sut.AnalyzeDocumentAsync(BuildUploadFileDto(), CancellationToken.None));
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_WhenBadRequest_ShouldThrowHttpRequestException()
    {
        // Arrange
        _handler.RespondWith(HttpStatusCode.BadRequest);

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(() =>
            _sut.AnalyzeDocumentAsync(BuildUploadFileDto(), CancellationToken.None));
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_WhenUnauthorized_ShouldThrowHttpRequestException()
    {
        // Arrange
        _handler.RespondWith(HttpStatusCode.Unauthorized);

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(() =>
            _sut.AnalyzeDocumentAsync(BuildUploadFileDto(), CancellationToken.None));
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_WhenNullJsonResponse_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _handler.RespondWith(HttpStatusCode.OK, JsonContent<DocumentProcessingResponse?>(null));

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.AnalyzeDocumentAsync(BuildUploadFileDto(), CancellationToken.None));
    }

    // ── ExpandAnalyzedDocument — request shape ───────────────────────────────

    [Fact]
    public async Task ExpandAnalyzedDocument_ShouldPostToCorrectEndpoint()
    {
        // Arrange
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildChapterDetailResponse()));

        // Act
        await _sut.ExpandAnalyzedDocument(
            new DocumentChapterRequest("OOP", "Classes", "doc-123"),
            CancellationToken.None);

        // Assert
        _handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        _handler.LastRequest.RequestUri!.PathAndQuery.ShouldBe("/document-courses/chapters/expand");
    }

    [Fact]
    public async Task ExpandAnalyzedDocument_ShouldSerializeRequestBodyWithSnakeCaseKeys()
    {
        // Arrange
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildChapterDetailResponse()));

        // Act
        await _sut.ExpandAnalyzedDocument(
            new DocumentChapterRequest("OOP", "Classes", "doc-123"),
            CancellationToken.None);

        // Assert — keys must be snake_case; PascalCase property names must not appear
        _handler.LastRequestBody.ShouldNotBeNull();
        _handler.LastRequestBody.ShouldContain("\"chapter_title\"");
        _handler.LastRequestBody.ShouldContain("\"core_concept\"");
        _handler.LastRequestBody.ShouldContain("\"document_id\"");
        _handler.LastRequestBody.ShouldNotContain("ChapterTitle");
        _handler.LastRequestBody.ShouldNotContain("CoreConcept");
        _handler.LastRequestBody.ShouldNotContain("DocumentId");
    }

    [Fact]
    public async Task ExpandAnalyzedDocument_ShouldSerializeRequestBodyWithCorrectValues()
    {
        // Arrange
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildChapterDetailResponse()));

        // Act
        await _sut.ExpandAnalyzedDocument(
            new DocumentChapterRequest("OOP", "Classes and Objects", "doc-456"),
            CancellationToken.None);

        // Assert
        _handler.LastRequestBody.ShouldNotBeNull();
        _handler.LastRequestBody.ShouldContain("\"OOP\"");
        _handler.LastRequestBody.ShouldContain("\"Classes and Objects\"");
        _handler.LastRequestBody.ShouldContain("\"doc-456\"");
    }

    // ── ExpandAnalyzedDocument — response deserialization ────────────────────

    [Fact]
    public async Task ExpandAnalyzedDocument_WhenSuccessResponse_ShouldReturnDeserializedSubchapters()
    {
        // Arrange
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildChapterDetailResponse()));

        // Act
        ChapterDetailResponse result = await _sut.ExpandAnalyzedDocument(
            new DocumentChapterRequest("OOP", "Classes", "doc-123"),
            CancellationToken.None);

        // Assert
        result.Subchapters.Count.ShouldBe(1);
        result.Subchapters[0].Title.ShouldBe("Classes");
        result.Subchapters[0].ContentSummary.ShouldBe("How to define classes");
        result.Subchapters[0].TheoryHtml.ShouldBe("<p>A class is a blueprint</p>");
    }

    [Fact]
    public async Task ExpandAnalyzedDocument_WhenSuccessResponse_ShouldReturnDeserializedSubchapterQuiz()
    {
        // Arrange
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildChapterDetailResponse()));

        // Act
        ChapterDetailResponse result = await _sut.ExpandAnalyzedDocument(
            new DocumentChapterRequest("OOP", "Classes", "doc-123"),
            CancellationToken.None);

        // Assert
        List<QuestionDto> quiz = result.Subchapters[0].Quiz;
        quiz.Count.ShouldBe(1);
        quiz[0].Question.ShouldBe("What is a class?");
        quiz[0].Options.Single(o => o.IsCorrect).Label.ShouldBe("A blueprint");
    }

    [Fact]
    public async Task ExpandAnalyzedDocument_WhenSuccessResponse_ShouldReturnDeserializedRecapQuiz()
    {
        // Arrange
        _handler.RespondWith(HttpStatusCode.OK, JsonContent(BuildChapterDetailResponse()));

        // Act
        ChapterDetailResponse result = await _sut.ExpandAnalyzedDocument(
            new DocumentChapterRequest("OOP", "Classes", "doc-123"),
            CancellationToken.None);

        // Assert
        result.RecapQuiz.Count.ShouldBe(1);
        result.RecapQuiz[0].Question.ShouldBe("What is OOP?");
        result.RecapQuiz[0].Options.Single(o => o.IsCorrect).Label.ShouldBe("A paradigm");
    }

    // ── ExpandAnalyzedDocument — error handling ──────────────────────────────

    [Fact]
    public async Task ExpandAnalyzedDocument_WhenNonSuccessResponse_ShouldThrowHttpRequestException()
    {
        // Arrange
        _handler.RespondWith(HttpStatusCode.InternalServerError);

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(() =>
            _sut.ExpandAnalyzedDocument(
                new DocumentChapterRequest("OOP", "Classes", "doc-123"),
                CancellationToken.None));
    }

    [Fact]
    public async Task ExpandAnalyzedDocument_WhenBadRequest_ShouldThrowHttpRequestException()
    {
        // Arrange
        _handler.RespondWith(HttpStatusCode.BadRequest);

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(() =>
            _sut.ExpandAnalyzedDocument(
                new DocumentChapterRequest("OOP", "Classes", "doc-123"),
                CancellationToken.None));
    }

    [Fact]
    public async Task ExpandAnalyzedDocument_WhenUnauthorized_ShouldThrowHttpRequestException()
    {
        // Arrange
        _handler.RespondWith(HttpStatusCode.Unauthorized);

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(() =>
            _sut.ExpandAnalyzedDocument(
                new DocumentChapterRequest("OOP", "Classes", "doc-123"),
                CancellationToken.None));
    }

    [Fact]
    public async Task ExpandAnalyzedDocument_WhenNullJsonResponse_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _handler.RespondWith(HttpStatusCode.OK, JsonContent<ChapterDetailResponse?>(null));

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.ExpandAnalyzedDocument(
                new DocumentChapterRequest("OOP", "Classes", "doc-123"),
                CancellationToken.None));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static StringContent JsonContent<T>(T value) =>
        new(JsonSerializer.Serialize(value, SnakeCaseOptions), Encoding.UTF8, "application/json");

    private static UploadFileDto BuildUploadFileDto() =>
        new("test.pdf", "application/pdf", new MemoryStream("file content"u8.ToArray()));

    private static DocumentProcessingResponse BuildDocumentProcessingResponse() =>
        new("doc-123",
            new DocumentAnalysisDto("C# Guide", "A comprehensive guide", ["OOP", "Async", "LINQ"]),
            new SkeletonDto("C# Programming", [
                new ChapterDto("Intro", "Overview", 1),
                new ChapterDto("OOP", "Classes", 2)
            ]),
            5);

    private static ChapterDetailResponse BuildChapterDetailResponse() =>
        new([
                new DetailedSubchapterDto(
                    "Classes",
                    "How to define classes",
                    "<p>A class is a blueprint</p>",
                    [
                        new QuestionDto("What is a class?", [
                            new QuestionOptionDto("A blueprint", true),
                            new QuestionOptionDto("A variable", false)
                        ])
                    ])
            ],
            [
                new QuestionDto("What is OOP?", [
                    new QuestionOptionDto("A paradigm", true),
                    new QuestionOptionDto("A language", false)
                ])
            ]);

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
                foreach (HttpContent part in multipart)
                {
                    MultipartParts.Add(new CapturedPart(
                        part.Headers.ContentDisposition?.Name?.Trim('"'),
                        part.Headers.ContentDisposition?.FileName?.Trim('"'),
                        part.Headers.ContentType?.MediaType));
                }
            }
            else if (request.Content is not null) LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(_statusCode) { Content = _responseContent };
        }
    }

    private sealed record CapturedPart(string? Name, string? FileName, string? ContentType);
}
