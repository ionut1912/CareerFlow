using System.Net;
using CareerFlow.Core.Api.IntegrationTests.Setup;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Api.IntegrationTests;

[Trait("Category", "Integration")]
public class LegalEndpointsTests : IntegrationTestBase
{
    public LegalEndpointsTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetLegalDoc_ShouldReturnOK_WhenValidType()
    {
        //Arrange
        var type = "privacy";

        //Act
        var response = await AnonymousClient.GetAsync($"/legal?type={type}");

        //Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetLegalDoc_ShouldReturnBadRequest_WhenInvalidType()
    {
        //Arrange
        var type = "test";

        //Act
        var response = await AnonymousClient.GetAsync($"/legal?type={type}");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}