using CareerFlow.Core.Api.IntegrationTests.Infrastructure;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Requests;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CareerFlow.Core.Api.IntegrationTests;

[Collection("Legal")]
public sealed class LegalEndpointGroupIntegrationTests : IntegrationTestBase
{
    private const string BaseUrl = "/legal";

    public LegalEndpointGroupIntegrationTests(SharedContainerFixture containers)
        : base(containers) { }

    // ── POST /api/legal — CreateLegalDoc ─────────────────────────────────────

    [Fact]
    public async Task CreateLegalDoc_ValidRequest_Returns200OkWithNonEmptyGuid()
    {
        // Arrange
        var client = CreateClient();
        var request = new LegalRequest("privacy-policy-create", "Privacy content.");

        // Act
        var response = await client.PostAsJsonAsync(BaseUrl, request);

        // Assert
        var id = await ShouldBeOkWithAsync<Guid>(response);
        id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateLegalDoc_ValidRequest_DocumentCanBeRetrievedAfterCreation()
    {
        // Arrange
        var client = CreateClient();
        const string type = "terms-persist-check";
        const string content = "Terms content to verify.";

        // Act
        await client.PostAsJsonAsync(BaseUrl, new LegalRequest(type, content));
        var response = await client.GetAsync($"{BaseUrl}?type={type}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain(WebUtility.HtmlEncode(content));
    }

    [Fact]
    public async Task CreateLegalDoc_DuplicateType_Returns4xx()
    {
        // Arrange
        var client = CreateClient();
        var request = new LegalRequest("duplicate-type-legal", "Content.");

        // Act
        var first = await client.PostAsJsonAsync(BaseUrl, request);
        first.IsSuccessStatusCode.ShouldBeTrue();
        var second = await client.PostAsJsonAsync(BaseUrl, request);

        // Assert
        ((int)second.StatusCode).ShouldBeInRange(400, 422);
    }

    [Fact]
    public async Task CreateLegalDoc_EmptyContent_Returns400BadRequest()
    {
        // Arrange
        var client = CreateClient();
        var request = new LegalRequest("empty-content-legal", string.Empty);

        // Act
        var response = await client.PostAsJsonAsync(BaseUrl, request);

        // Assert
        ShouldBeBadRequest(response);
    }

    [Fact]
    public async Task CreateLegalDoc_EmptyType_Returns400BadRequest()
    {
        // Arrange
        var client = CreateClient();
        var request = new LegalRequest(string.Empty, "Some content.");

        // Act
        var response = await client.PostAsJsonAsync(BaseUrl, request);

        // Assert
        ShouldBeBadRequest(response);
    }

    [Fact]
    public async Task CreateLegalDoc_MissingBody_Returns400BadRequest()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.PostAsync(BaseUrl, content: null);

        // Assert
        ShouldBeBadRequest(response);
    }

    // ── PUT /api/legal — UpdateLegalDoc ──────────────────────────────────────

    [Fact]
    public async Task UpdateLegalDoc_ExistingDocument_Returns200OkWithUpdatedDto()
    {
        // Arrange
        var client = CreateClient();
        const string type = "update-happy-legal";
        await SeedDocumentAsync(client, type, "Original content.");

        // Act
        var response = await client.PutAsJsonAsync(BaseUrl, new LegalRequest(type, "Updated content."));

        // Assert
        var dto = await ShouldBeOkWithAsync<LegalDocDto>(response);
        dto.Type.ShouldBe(type);
        dto.Content.ShouldBe("Updated content.");
    }

    [Fact]
    public async Task UpdateLegalDoc_ExistingDocument_GetReturnsNewContentAfterUpdate()
    {
        // Arrange
        var client = CreateClient();
        const string type = "update-durability-legal";
        await SeedDocumentAsync(client, type, "Original.");
        await client.PutAsJsonAsync(BaseUrl, new LegalRequest(type, "Durable update."));

        // Act
        var response = await client.GetAsync($"{BaseUrl}?type={type}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain(WebUtility.HtmlEncode("Durable update."));
    }

    [Fact]
    public async Task UpdateLegalDoc_NonExistentDocument_Returns404NotFound()
    {
        // Arrange
        var client = CreateClient();
        var request = new LegalRequest("does-not-exist-type-xyz", "content");

        // Act
        var response = await client.PutAsJsonAsync(BaseUrl, request);

        // Assert
        await ShouldBeNotFoundAsync(response);
    }

    [Fact]
    public async Task UpdateLegalDoc_EmptyContent_Returns400BadRequest()
    {
        // Arrange
        var client = CreateClient();
        var request = new LegalRequest("any-legal-type", string.Empty);

        // Act
        var response = await client.PutAsJsonAsync(BaseUrl, request);

        // Assert
        ShouldBeBadRequest(response);
    }

    [Fact]
    public async Task UpdateLegalDoc_MissingBody_Returns400BadRequest()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.PutAsync(BaseUrl, content: null);

        // Assert
        ShouldBeBadRequest(response);
    }

    // ── GET /api/legal?type=... — GetLegalDoc ────────────────────────────────

    [Fact]
    public async Task GetLegalDoc_ExistingType_Returns200OkWithTextHtmlContentType()
    {
        // Arrange
        var client = CreateClient();
        const string type = "get-content-type-legal";
        await SeedDocumentAsync(client, type, "Privacy content.");

        // Act
        var response = await client.GetAsync($"{BaseUrl}?type={type}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        ShouldHaveContentType(response, "text/html");
    }

    [Fact]
    public async Task GetLegalDoc_ExistingType_BodyContainsHtmlEncodedContent()
    {
        // Arrange
        var client = CreateClient();
        const string type = "get-xss-legal";
        const string content = "<script>alert('xss')</script> & \"quoted\"";
        await SeedDocumentAsync(client, type, content);

        // Act
        var response = await client.GetAsync($"{BaseUrl}?type={type}");
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        body.ShouldNotContain(content);
        body.ShouldContain(WebUtility.HtmlEncode(content));
    }

    [Fact]
    public async Task GetLegalDoc_ExistingType_HtmlBodyContainsCareerFlowBrandingAndCurrentYear()
    {
        // Arrange
        var client = CreateClient();
        const string type = "get-branding-legal";
        await SeedDocumentAsync(client, type, "Content.");

        // Act
        var response = await client.GetAsync($"{BaseUrl}?type={type}");
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        body.ShouldContain("CareerFlow");
        body.ShouldContain("<!DOCTYPE html>");
        body.ShouldContain(DateTime.Now.Year.ToString());
    }

    [Fact]
    public async Task GetLegalDoc_TypeDoesNotExist_Returns404WithRomanianMessage()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.GetAsync($"{BaseUrl}?type=nonexistent-policy-xyz");

        // Assert
        await ShouldBeNotFoundWithBodyAsync(response, "gasita");
    }

    [Fact]
    public async Task GetLegalDoc_MissingTypeQueryParameter_Returns400BadRequest()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.GetAsync(BaseUrl);

        // Assert
        ShouldBeBadRequest(response);
    }

    [Fact]
    public async Task GetLegalDoc_EmptyTypeQueryParameter_Returns404OrBadRequest()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.GetAsync($"{BaseUrl}?type=");

        // Assert
        response.StatusCode.ShouldBeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound);
    }

    // ── Full round-trip ───────────────────────────────────────────────────────

    [Fact]
    public async Task LegalDoc_FullRoundTrip_CreateUpdateGet_DataIsConsistent()
    {
        // Arrange
        var client = CreateClient();
        const string type = "round-trip-legal";

        // Act
        var createResp = await client.PostAsJsonAsync(BaseUrl, new LegalRequest(type, "Draft."));
        var newId = await ShouldBeOkWithAsync<Guid>(createResp);

        var updateResp = await client.PutAsJsonAsync(BaseUrl, new LegalRequest(type, "Final version."));
        var dto = await ShouldBeOkWithAsync<LegalDocDto>(updateResp);

        var getResp = await client.GetAsync($"{BaseUrl}?type={type}");
        var body = await getResp.Content.ReadAsStringAsync();

        // Assert
        newId.ShouldNotBe(Guid.Empty);
        dto.Content.ShouldBe("Final version.");
        body.ShouldContain(WebUtility.HtmlEncode("Final version."));
    }

    [Fact]
    public async Task CreateLegalDoc_MultipleDistinctTypes_AllReturnUniqueGuids()
    {
        // Arrange
        var client = CreateClient();
        var types = new[] { "cookie-policy-multi", "gdpr-notice-multi", "accessibility-multi" };

        // Act
        var tasks = types.Select(t => client.PostAsJsonAsync(BaseUrl, new LegalRequest(t, $"Content for {t}.")));
        var responses = await Task.WhenAll(tasks);

        // Assert
        var ids = new List<Guid>();
        foreach (var response in responses)
        {
            var id = await ShouldBeOkWithAsync<Guid>(response);
            id.ShouldNotBe(Guid.Empty);
            ids.Add(id);
        }
        ids.Distinct().Count().ShouldBe(types.Length);
    }

    private async Task SeedDocumentAsync(HttpClient client, string type, string content)
    {
        var response = await client.PostAsJsonAsync(BaseUrl, new LegalRequest(type, content));
        response.IsSuccessStatusCode.ShouldBeTrue(
            $"Seed failed for type='{type}': {(int)response.StatusCode}");
    }
}