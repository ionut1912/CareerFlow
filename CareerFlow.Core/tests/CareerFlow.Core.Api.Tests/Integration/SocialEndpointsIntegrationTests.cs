using System.Net;
using CareerFlow.Core.Api.Tests.Setup;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Api.Tests.Integration;

[Trait("Category", "Integration")]
public class SocialEndpointsIntegrationTests : IntegrationTestBase
{
    public SocialEndpointsIntegrationTests(TestWebApplicationFactory factory) : base(factory) { }
 
    private HttpClient NoRedirectClient() => Factory.CreateClient(
        new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
 
    [Fact]
    public async Task GoogleMobileLogin_NoParams_Returns302Redirect()
    {
        var response = await NoRedirectClient().GetAsync("/social/auth/google/mobile");
 
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
    }
 
    [Fact]
    public async Task GoogleMobileLogin_WithReturnUrl_Returns302Redirect()
    {
        var response = await NoRedirectClient().GetAsync("/social/auth/google/mobile?returnUrl=myapp://callback");
 
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
    }
 
    [Fact]
    public async Task GoogleMobileLogin_LocationHeaderPointsToGoogle()
    {
        var response = await NoRedirectClient().GetAsync("/social/auth/google/mobile");
 
        var location = response.Headers.Location?.ToString();
        location.ShouldNotBeNullOrWhiteSpace();
        location.ShouldContain("accounts.google.com");
    }
 
    [Fact]
    public async Task GoogleMobileLogin_ContainsStateInRedirectUrl()
    {
        var response = await NoRedirectClient().GetAsync("/social/auth/google/mobile");
 
        var location = response.Headers.Location?.ToString();
        location.ShouldContain("state=");
    }
 
    [Fact]
    public async Task GoogleMobileLogin_TwoConsecutiveCalls_ProduceDifferentStates()
    {
        var client = NoRedirectClient();
 
        var r1 = await client.GetAsync("/social/auth/google/mobile");
        var r2 = await client.GetAsync("/social/auth/google/mobile");
 
        var loc1 = r1.Headers.Location?.ToString();
        var loc2 = r2.Headers.Location?.ToString();
 
        loc1.ShouldNotBe(loc2);
    }
 
    [Fact]
    public async Task LinkedInMobileLogin_NoParams_Returns302Redirect()
    {
        var response = await NoRedirectClient().GetAsync("/social/auth/linkedin/mobile");
 
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
    }
 
    [Fact]
    public async Task LinkedInMobileLogin_LocationHeaderPointsToLinkedIn()
    {
        var response = await NoRedirectClient().GetAsync("/social/auth/linkedin/mobile");
 
        var location = response.Headers.Location?.ToString();
        location.ShouldNotBeNullOrWhiteSpace();
        location.ShouldContain("linkedin.com");
    }
 
    [Fact]
    public async Task LinkedInMobileLogin_ContainsStateInRedirectUrl()
    {
        var response = await NoRedirectClient().GetAsync("/social/auth/linkedin/mobile");
 
        var location = response.Headers.Location?.ToString();
        location.ShouldContain("state=");
    }
 
    [Fact]
    public async Task GoogleMobileCallback_InvalidState_Returns500OrBadRequest()
    {
        var response = await NoRedirectClient()
            .GetAsync("/social/auth/google/mobile/callback?code=abc&state=invalid-csrf-state");
 
        ((int)response.StatusCode).ShouldBeGreaterThanOrEqualTo(400);
    }
 
    [Fact]
    public async Task LinkedInMobileCallback_InvalidState_Returns500OrBadRequest()
    {
        var response = await NoRedirectClient()
            .GetAsync("/social/auth/linkedin/mobile/callback?code=abc&state=bad-state");
 
        ((int)response.StatusCode).ShouldBeGreaterThanOrEqualTo(400);
    }
 
    [Fact]
    public async Task GoogleMobileLogin_WithReturnUrl_HasClientIdInLocation()
    {
        var response = await NoRedirectClient()
            .GetAsync("/social/auth/google/mobile?returnUrl=myapp://callback");
 
        var location = response.Headers.Location?.ToString();
        location.ShouldContain("client_id");
    }
 
    [Fact]
    public async Task LinkedInMobileLogin_WithReturnUrl_HasClientIdInLocation()
    {
        var response = await NoRedirectClient()
            .GetAsync("/social/auth/linkedin/mobile?returnUrl=myapp://callback");
 
        var location = response.Headers.Location?.ToString();
        location.ShouldContain("client_id");
    }
}