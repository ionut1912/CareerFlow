using System.Net;
using System.Net.Http.Json;
using CareerFlow.Core.Api.IntegrationTests.Setup;
using CareerFlow.Core.Application.CQRS.Legal.Command;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Requests;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Api.IntegrationTests;

public class LegalEndpointsTests: IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;

    public LegalEndpointsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }
    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();

        var client = _factory.CreateClient();
        await client.PostAsJsonAsync("/legal",
            new CreateLegalDocCommand("testContent","PrivacyPolicy"));
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateLegalDoc_ShouldReturnOk_WhenValidRequest()
    {
        //Arrange
        var client=_factory.CreateClient();
        var request=new LegalRequest("testContent","PrivacyPolicy");
        
        //Act
        var response=await client.PostAsJsonAsync("/legal",request);
        
        //Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Guid>();
        result.ShouldNotBe(Guid.Empty);
    }
    
    [Fact]
    public async Task CreateLegalDoc_ShouldReturnBadRequest_WhenInvalidRequest()
    {
        //Arrange
        var client=_factory.CreateClient();
        var request = new LegalRequest(string.Empty, string.Empty);
        
        //Act
        var response=await client.PostAsJsonAsync("/legal",request);
        
        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateLegalDocAsync_ShouldReturnNotFound_WhenInvalidUri()
    {
        //Arrange
        var client = _factory.CreateClient();
        var request=new LegalRequest("testContent","PrivacyPolicy");
        
        //Act
        var response=await client.PostAsJsonAsync("/invalidurl",request);
        
        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLegalDoc_ShouldReturnOk_WhenLegalDocExists()
    {
        //Arrange
        var client = _factory.CreateClient();
        var type="PrivacyPolicy";
        //Act
        var response=await client.GetAsync($"/legal?type={type}");
        
        //Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task GetLegalDoc_ShouldReturnNotFound_WhenLegalDocDoesNotExist()
    {
        //Arrange
        var client = _factory.CreateClient();
        var type="TermsAndConditions";
        
        //Act
        var response=await client.GetAsync($"/legal?type={type}");
        
        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLegalDoc_ShouldReturnBadRequest_WhenInvalidType()
    {
        
            //Arrange
            var client = _factory.CreateClient();
            var type="Testtype";
        
            //Act
            var response=await client.GetAsync($"/legal?type={type}");
        
            //Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetLegaDocsAsync_ShouldReturnNotFound_WhenInvalidUrl()
    {
        //Arrange
        var client = _factory.CreateClient();
        var type="PrivacyPolicy";
        //Act
        var response=await client.GetAsync("invalidurl");
        
        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateLegalDocDto_ShouldReturnUpdatedDoc_WhenValidRequest()
    {
        //Arrange
        var client=_factory.CreateClient();
        var request=new LegalRequest("testContent","PrivacyPolicy");
        
        //Act
        var  response=await client.PutAsJsonAsync("/legal",request);
        
        //Assert
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
        //Arrange
        var client=_factory.CreateClient();
        var request=new LegalRequest(string.Empty,string.Empty);
        
        //Act
        var  response=await client.PutAsJsonAsync("/legal",request);
        
        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateLegalDocDtoAsync_ShouldReturnNotFound_WhenInvalidUri()
    {
        //Arrange
        var client=_factory.CreateClient();
        var request=new LegalRequest("testContent","TermsAndConditions");
        
        //Act
        var  response=await client.PutAsJsonAsync("/invalid-url",request);
        
        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}