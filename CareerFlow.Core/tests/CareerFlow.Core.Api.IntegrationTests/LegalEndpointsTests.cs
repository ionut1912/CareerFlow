using System.Net;
using CareerFlow.Core.Api.IntegrationTests.Setup;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Api.IntegrationTests;

[Trait("Category", "Integration")]
public class LegalEndpointsTests:IntegrationTestBase
{
    public LegalEndpointsTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetLegalDoc_ShouldReturnOK_WhenValidType()
    {
        //Arrange
        var client = Factory.CreateClient();
        var type = "privacy";
        
        //Act
        var responese=await client.GetAsync($"/legal?type={type}");
        
        //Assert
        responese.EnsureSuccessStatusCode();
        responese.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task GetLegalDoc_ShouldReturnBadRequest_WhenInvalidType()
    {
        //Arrange
        var client = Factory.CreateClient();
        var type = "test";

        //Act
        var responese = await client.GetAsync($"/legal?type={type}");
        
        //Assert
        responese.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
    
    
}