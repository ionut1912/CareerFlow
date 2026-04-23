using System.Net;

using CareerFlow.Core.Api.Tests.Setup;

using Shouldly;

using Xunit;

namespace CareerFlow.Core.Api.Tests.Integration;

[Trait("Category", "Integration")]
public class LegalEndpointsTests(TestWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetLegalDoc_ShouldReturnOK_WhenValidType()
    {
        //Arrange
        const string type = "privacy";

        //Act
        HttpResponseMessage response = await AnonymousClient.GetAsync($"/legal?type={type}");

        //Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetLegalDoc_ShouldReturnBadRequest_WhenInvalidType()
    {
        //Arrange
        const string type = "test";

        //Act
        HttpResponseMessage response = await AnonymousClient.GetAsync($"/legal?type={type}");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
