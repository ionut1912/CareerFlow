using System.Net;
using System.Net.Http.Json;
using CareerFlow.Core.Api.IntegrationTests.Setup;
using CareerFlow.Core.Application.CQRS.Legal.Command;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Requests;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Api.IntegrationTests;

[Trait("Category", "Integration")]
public class LegalEndpointsTests : IntegrationTestBase
{
    public LegalEndpointsTests(TestWebApplicationFactory factory) : base(factory) { }

    private async Task SeedLegalDocAsync(string content, string type)
    {
        var command = new CreateLegalDocCommand(content, type);
        await AnonymousClient.PostAsJsonAsync("/legal", command);
    }

    [Fact]
    public async Task CreateLegalDoc_ShouldReturnOk_WhenValidRequest()
    {
        var request = new LegalRequest("testContent", "PrivacyPolicy");
        
        var response = await AnonymousClient.PostAsJsonAsync("/legal", request);
        
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Guid>();
        result.ShouldNotBe(Guid.Empty);
    }
    
    [Fact]
    public async Task CreateLegalDoc_ShouldReturnBadRequest_WhenInvalidRequest()
    {
        var request = new LegalRequest(string.Empty, string.Empty);
        
        var response = await AnonymousClient.PostAsJsonAsync("/legal", request);
        
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateLegalDocAsync_ShouldReturnNotFound_WhenInvalidUri()
    {
        var request = new LegalRequest("testContent", "PrivacyPolicy");
        
        var response = await AnonymousClient.PostAsJsonAsync("/invalidurl", request);
        
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLegalDoc_ShouldReturnOk_WhenLegalDocExists()
    {
        await SeedLegalDocAsync("testContent", "PrivacyPolicy");
        var type = "PrivacyPolicy";

        var response = await AnonymousClient.GetAsync($"/legal?type={type}");
        
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task GetLegalDoc_ShouldReturnNotFound_WhenLegalDocDoesNotExist()
    {
        var type = "TermsAndConditions";
        
        var response = await AnonymousClient.GetAsync($"/legal?type={type}");
        
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLegalDoc_ShouldReturnBadRequest_WhenInvalidType()
    {
        var type = "Testtype";
        
        var response = await AnonymousClient.GetAsync($"/legal?type={type}");
        
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetLegaDocsAsync_ShouldReturnNotFound_WhenInvalidUrl()
    {
        var response = await AnonymousClient.GetAsync("invalidurl");
        
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateLegalDocDto_ShouldReturnUpdatedDoc_WhenValidRequest()
    {
        await SeedLegalDocAsync("initialContent", "PrivacyPolicy");
        var request = new LegalRequest("testContent", "PrivacyPolicy");
        
        var response = await AnonymousClient.PutAsJsonAsync("/legal", request);
        
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LegalDocDto>();
        result.ShouldNotBeNull();
        result.Content.ShouldBe(request.Content);
        result.Type.ShouldBe(request.Type);
    }

    [Fact]
    public async Task UpdateLegalDocDto_ShouldReturnBadRequest_WhenInvalidRequest()
    {
        var request = new LegalRequest(string.Empty, string.Empty);
        
        var response = await AnonymousClient.PutAsJsonAsync("/legal", request);
        
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateLegalDocDtoAsync_ShouldReturnNotFound_WhenInvalidUri()
    {
        var request = new LegalRequest("testContent", "TermsAndConditions");
        
        var response = await AnonymousClient.PutAsJsonAsync("/invalid-url", request);
        
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}