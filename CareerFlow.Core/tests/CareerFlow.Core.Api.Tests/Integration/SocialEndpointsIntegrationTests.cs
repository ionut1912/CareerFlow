using System.Net;
using CareerFlow.Core.Api.Tests.Setup;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Api.Tests.Integration;

[Trait("Category", "Integration")]
public class SocialEndpointsIntegrationTests(TestWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private HttpClient NoRedirectClient()
    {
        return Factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    [Fact]
    public async Task GoogleMobileLogin_NoParams_Returns302Redirect()
    {
        HttpResponseMessage response = await NoRedirectClient().GetAsync("/social/auth/google/mobile");

        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task GoogleMobileLogin_WithReturnUrl_Returns302Redirect()
    {
        HttpResponseMessage response = await NoRedirectClient().GetAsync("/social/auth/google/mobile?returnUrl=myapp://callback");

        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task GoogleMobileLogin_LocationHeaderPointsToGoogle()
    {
        HttpResponseMessage response = await NoRedirectClient().GetAsync("/social/auth/google/mobile");

        string? location = response.Headers.Location?.ToString();
        location.ShouldNotBeNullOrWhiteSpace();
        location.ShouldContain("accounts.google.com");
    }

    [Fact]
    public async Task GoogleMobileLogin_ContainsStateInRedirectUrl()
    {
        HttpResponseMessage response = await NoRedirectClient().GetAsync("/social/auth/google/mobile");

        string? location = response.Headers.Location?.ToString();
        location.ShouldNotBeNullOrWhiteSpace();
        location.ShouldContain("state=");
    }

    [Fact]
    public async Task GoogleMobileLogin_TwoConsecutiveCalls_ProduceDifferentStates()
    {
        HttpClient client = NoRedirectClient();

        HttpResponseMessage r1 = await client.GetAsync("/social/auth/google/mobile");
        HttpResponseMessage r2 = await client.GetAsync("/social/auth/google/mobile");

        string? loc1 = r1.Headers.Location?.ToString();
        string? loc2 = r2.Headers.Location?.ToString();

        loc1.ShouldNotBe(loc2);
    }

    [Fact]
    public async Task LinkedInMobileLogin_NoParams_Returns302Redirect()
    {
        HttpResponseMessage response = await NoRedirectClient().GetAsync("/social/auth/linkedin/mobile");

        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task LinkedInMobileLogin_LocationHeaderPointsToLinkedIn()
    {
        HttpResponseMessage response = await NoRedirectClient().GetAsync("/social/auth/linkedin/mobile");

        string? location = response.Headers.Location?.ToString();
        location.ShouldNotBeNullOrWhiteSpace();
        location.ShouldContain("linkedin.com");
    }

    [Fact]
    public async Task LinkedInMobileLogin_ContainsStateInRedirectUrl()
    {
        HttpResponseMessage response = await NoRedirectClient().GetAsync("/social/auth/linkedin/mobile");

        string? location = response.Headers.Location?.ToString();
        location.ShouldNotBeNull();
        location.ShouldContain("state=");
    }

    [Fact]
    public async Task LinkedInMobileLogin_WithReturnUrl_HasClientIdInLocation()
    {
        HttpResponseMessage response = await NoRedirectClient()
            .GetAsync("/social/auth/linkedin/mobile?returnUrl=myapp://callback");

        string? location = response.Headers.Location?.ToString();
        location.ShouldNotBeNull();
        location.ShouldContain("client_id");
    }
}
