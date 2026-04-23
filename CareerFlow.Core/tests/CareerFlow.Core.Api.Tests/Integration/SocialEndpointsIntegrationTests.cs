using System.Net;

using CareerFlow.Core.Api.Tests.Setup;

using Microsoft.AspNetCore.Mvc.Testing;

using Shouldly;

using Xunit;

namespace CareerFlow.Core.Api.Tests.Integration;

[Trait("Category", "Integration")]
public class SocialEndpointsIntegrationTests(TestWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private HttpClient CreateNoRedirectClient() =>
        Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    [Fact]
    public async Task GoogleMobileLogin_NoParams_Returns302Redirect()
    {
        //Arrange
        using HttpClient client = CreateNoRedirectClient();

        //Act
        HttpResponseMessage response = await client.GetAsync("/social/auth/google/mobile");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task GoogleMobileLogin_WithReturnUrl_Returns302Redirect()
    {
        //Arrange
        using HttpClient client = CreateNoRedirectClient();

        //Act
        HttpResponseMessage response = await client.GetAsync("/social/auth/google/mobile?returnUrl=myapp://callback");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task GoogleMobileLogin_LocationHeaderPointsToGoogle()
    {
        //Arrange
        using HttpClient client = CreateNoRedirectClient();

        //Act
        HttpResponseMessage response = await client.GetAsync("/social/auth/google/mobile");

        //Assert
        string? location = response.Headers.Location?.ToString();
        location.ShouldNotBeNullOrWhiteSpace();
        location.ShouldContain("accounts.google.com");
    }

    [Fact]
    public async Task GoogleMobileLogin_ContainsStateInRedirectUrl()
    {
        //Arrange
        using HttpClient client = CreateNoRedirectClient();

        //Act
        HttpResponseMessage response = await client.GetAsync("/social/auth/google/mobile");

        //Assert
        string? location = response.Headers.Location?.ToString();
        location.ShouldNotBeNullOrWhiteSpace();
        location.ShouldContain("state=");
    }

    [Fact]
    public async Task GoogleMobileLogin_TwoConsecutiveCalls_ProduceDifferentStates()
    {
        //Arrange
        using HttpClient client = CreateNoRedirectClient();

        //Act
        HttpResponseMessage r1 = await client.GetAsync("/social/auth/google/mobile");
        HttpResponseMessage r2 = await client.GetAsync("/social/auth/google/mobile");

        //Assert
        string? loc1 = r1.Headers.Location?.ToString();
        string? loc2 = r2.Headers.Location?.ToString();

        loc1.ShouldNotBe(loc2);
    }

    [Fact]
    public async Task LinkedInMobileLogin_NoParams_Returns302Redirect()
    {
        //Arrange
        using HttpClient client = CreateNoRedirectClient();

        //Act
        HttpResponseMessage response = await client.GetAsync("/social/auth/linkedin/mobile");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task LinkedInMobileLogin_LocationHeaderPointsToLinkedIn()
    {
        //Arrange
        using HttpClient client = CreateNoRedirectClient();

        //Act
        HttpResponseMessage response = await client.GetAsync("/social/auth/linkedin/mobile");

        //Assert
        string? location = response.Headers.Location?.ToString();
        location.ShouldNotBeNullOrWhiteSpace();
        location.ShouldContain("linkedin.com");
    }

    [Fact]
    public async Task LinkedInMobileLogin_ContainsStateInRedirectUrl()
    {
        //Arrange
        using HttpClient client = CreateNoRedirectClient();

        //Act
        HttpResponseMessage response = await client.GetAsync("/social/auth/linkedin/mobile");

        //Assert
        string? location = response.Headers.Location?.ToString();
        location.ShouldNotBeNull();
        location.ShouldContain("state=");
    }

    [Fact]
    public async Task LinkedInMobileLogin_WithReturnUrl_HasClientIdInLocation()
    {
        //Arrange
        using HttpClient client = CreateNoRedirectClient();

        //Act
        HttpResponseMessage response = await client.GetAsync("/social/auth/linkedin/mobile?returnUrl=myapp://callback");

        //Assert
        string? location = response.Headers.Location?.ToString();
        location.ShouldNotBeNull();
        location.ShouldContain("client_id");
    }
}
