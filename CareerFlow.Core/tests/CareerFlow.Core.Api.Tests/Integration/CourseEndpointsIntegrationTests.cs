using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

using CareerFlow.Core.Api.Tests.Setup;

using Shouldly;

using Xunit;

namespace CareerFlow.Core.Api.Tests.Integration;

[Trait("Category", "Integration")]
public class CourseEndpointsIntegrationTests(TestWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task UploadCourse_UnauthenticatedUser_Returns401()
    {
        using MultipartFormDataContent content = CreateMultipartWithPdf("My Course");

        HttpResponseMessage response = await AnonymousClient.PostAsync("/course/upload", content);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadCourse_AuthenticatedUserMissingTitle_Returns400BadRequest()
    {
        (HttpClient authClient, _, _) = await CreateAndAuthenticateUserAsync();
        using var content = new MultipartFormDataContent();
        byte[] fileBytes = CreateMinimalPdf();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "files", "course.pdf");

        HttpResponseMessage response = await authClient.PostAsync("/course/upload", content);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadCourse_AuthenticatedUserNoFiles_Returns400BadRequest()
    {
        (HttpClient authClient, _, _) = await CreateAndAuthenticateUserAsync();
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("My Course"), "Title");

        HttpResponseMessage response = await authClient.PostAsync("/course/upload", content);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadCourse_AuthenticatedUserInvalidExtension_Returns202WithErrors()
    {
        (HttpClient authClient, _, _) = await CreateAndAuthenticateUserAsync();
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("My Course"), "Title");
        var fileContent = new ByteArrayContent([1, 2, 3]);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "files", "malware.exe");

        HttpResponseMessage response = await authClient.PostAsync("/course/upload", content);

        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        string body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("malware.exe");
    }

    [Fact]
    public async Task UploadCourse_AuthenticatedUserOversizedFile_Returns202WithErrors()
    {
        (HttpClient authClient, _, _) = await CreateAndAuthenticateUserAsync();
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("My Course"), "Title");
        var oversized = new ByteArrayContent(new byte[21 * 1024 * 1024]);
        oversized.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(oversized, "files", "huge.pdf");

        HttpResponseMessage response = await authClient.PostAsync("/course/upload", content);

        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        string body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("huge.pdf");
    }

    [Fact]
    public async Task FinishChapter_UnauthenticatedUser_Returns401()
    {
        HttpResponseMessage response = await AnonymousClient.PostAsync(
            $"/course/{Guid.NewGuid()}/chapters/{Guid.NewGuid()}/finish", null);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task FinishChapter_AuthenticatedUserChapterNotFound_ReturnsErrorStatus()
    {
        (HttpClient authClient, _, _) = await CreateAndAuthenticateUserAsync();

        HttpResponseMessage response = await authClient.PostAsync(
            $"/course/{Guid.NewGuid()}/chapters/{Guid.NewGuid()}/finish", null);

        ((int)response.StatusCode).ShouldBeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GenerateCourse_UnauthenticatedUser_Returns401()
    {
        HttpResponseMessage response = await AnonymousClient.PostAsJsonAsync("/course/generate", new { Topic = "C#" });

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GenerateCourse_AuthenticatedUserEmptyTopic_Returns400()
    {
        (HttpClient authClient, _, _) = await CreateAndAuthenticateUserAsync();

        HttpResponseMessage response = await authClient.PostAsJsonAsync("/course/generate", new { Topic = "" });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private static MultipartFormDataContent CreateMultipartWithPdf(string title)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(title), "Title");
        var fileContent = new ByteArrayContent(CreateMinimalPdf());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "files", "course.pdf");
        return content;
    }

    private static byte[] CreateMinimalPdf()
    {
        const string pdf =
            "%PDF-1.4\n1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj\n2 0 obj<</Type/Pages/Kids[3 0 R]/Count 1>>endobj\n3 0 obj<</Type/Page/MediaBox[0 0 612 792]>>endobj\nxref\n0 4\ntrailer<</Size 4/Root 1 0 R>>\nstartxref\n190\n%%EOF";
        return Encoding.ASCII.GetBytes(pdf);
    }
}
