using System.Net;
using System.Net.Http.Json;
using CareerFlow.Core.Api.Tests.Setup;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Requests.UserProfile;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Api.Tests.Integration;

[Trait("Category", "Integration")]
public class UserProfileEndpointsIntegrationTests : IntegrationTestBase
{
    public UserProfileEndpointsIntegrationTests(TestWebApplicationFactory factory) : base(factory) { }
 
    [Fact]
    public async Task CreateUserProfile_ValidRequest_Returns200WithProfileId()
    {
        var (authClient, _, _) = await CreateAndAuthenticateUserAsync();
        var request = new CreateUserProfileRequest("Visual", ["Student"], string.Empty);
 
        var response = await authClient.PostAsJsonAsync("/user-profile", request);
        var result = await response.Content.ReadFromJsonAsync<Guid>();
 
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBe(Guid.Empty);
    }
 
    [Fact]
    public async Task CreateUserProfile_InvalidLearningType_Returns400()
    {
        var (authClient, _, _) = await CreateAndAuthenticateUserAsync();
 
        var response = await authClient.PostAsJsonAsync("/user-profile",
            new CreateUserProfileRequest("InvalidType", ["Student"], string.Empty));
 
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
 
    [Fact]
    public async Task CreateUserProfile_Unauthenticated_Returns401()
    {
        var response = await AnonymousClient.PostAsJsonAsync("/user-profile",
            new CreateUserProfileRequest("Visual", ["Student"], string.Empty));
 
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
 
    [Fact]
    public async Task CreateUserProfile_DuplicateForSameAccount_Returns500()
    {
        var (authClient, _, _) = await CreateAndAuthenticateUserAsync();
        var request = new CreateUserProfileRequest("Visual", ["Student"], string.Empty);
        await authClient.PostAsJsonAsync("/user-profile", request);
 
        var response = await authClient.PostAsJsonAsync("/user-profile", request);
 
        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
    }
 
    [Fact]
    public async Task GetUserProfiles_WithCreatedProfile_Returns200WithNonEmptyList()
    {
        var (authClient, account, _) = await CreateAndAuthenticateUserAsync();
        await CreateProfile(authClient, new CreateUserProfileRequest("Visual", ["Student"], string.Empty));
 
        var response = await authClient.GetAsync("/user-profile");
        var result = await response.Content.ReadFromJsonAsync<List<UserProfileDto>>();
 
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBeEmpty();
        result.ShouldContain(p => p.AccountId == account.Id);
    }
 
    [Fact]
    public async Task GetUserProfiles_Unauthenticated_Returns401()
    {
        var response = await AnonymousClient.GetAsync("/user-profile");
 
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
 
    [Fact]
    public async Task GetUserProfile_ExistingId_Returns200WithCorrectData()
    {
        var (authClient, account, _) = await CreateAndAuthenticateUserAsync();
        var request = new CreateUserProfileRequest("Visual", ["Student"], string.Empty);
        var id = await CreateProfile(authClient, request);
 
        var response = await authClient.GetAsync($"/user-profile/{id}");
        var result = await response.Content.ReadFromJsonAsync<UserProfileDto>();
 
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(id);
        result.AccountId.ShouldBe(account.Id);
        result.LearningType.ShouldBe("Visual");
        result.Domain.ShouldBe(string.Empty);
    }
 
    [Fact]
    public async Task GetUserProfile_NonExistingId_Returns404()
    {
        var (authClient, _, _) = await CreateAndAuthenticateUserAsync();
 
        var response = await authClient.GetAsync($"/user-profile/{Guid.NewGuid()}");
 
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
 
    [Fact]
    public async Task GetUserProfile_Unauthenticated_Returns401()
    {
        var response = await AnonymousClient.GetAsync($"/user-profile/{Guid.NewGuid()}");
 
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
 
    [Fact]
    public async Task GetCurrentUserProfile_WithProfile_Returns200WithCorrectAccountId()
    {
        var (authClient, account, _) = await CreateAndAuthenticateUserAsync();
        await CreateProfile(authClient, new CreateUserProfileRequest("Visual", ["Student"], string.Empty));
 
        var response = await authClient.GetAsync("/user-profile/current");
        var result = await response.Content.ReadFromJsonAsync<UserProfileDto>();
 
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBeNull();
        result.AccountId.ShouldBe(account.Id);
    }
 
    [Fact]
    public async Task GetCurrentUserProfile_WithoutProfile_Returns4xx()
    {
        var (authClient, _, _) = await CreateAndAuthenticateUserAsync();
 
        var response = await authClient.GetAsync("/user-profile/current");
 
        ((int)response.StatusCode).ShouldBeGreaterThanOrEqualTo(400);
    }
 
    [Fact]
    public async Task GetCurrentUserProfile_Unauthenticated_Returns401()
    {
        var response = await AnonymousClient.GetAsync("/user-profile/current");
 
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
 
    [Fact]
    public async Task GetCurrentUserProfileWithCourses_WithProfile_Returns200()
    {
        var (authClient, _, _) = await CreateAndAuthenticateUserAsync();
        await CreateProfile(authClient, new CreateUserProfileRequest("Visual", ["Student"], string.Empty));
 
        var response = await authClient.GetAsync("/user-profile/current/with-courses");
 
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
 
    [Fact]
    public async Task GetCurrentUserProfileWithCourses_Unauthenticated_Returns401()
    {
        var response = await AnonymousClient.GetAsync("/user-profile/current/with-courses");
 
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
 
    [Fact]
    public async Task UpdateUserProfile_ValidRequest_Returns204NoContent()
    {
        var (authClient, _, _) = await CreateAndAuthenticateUserAsync();
        var id = await CreateProfile(authClient, new CreateUserProfileRequest("Visual", ["Student"], string.Empty));
 
        var response = await authClient.PutAsJsonAsync($"/user-profile/{id}",
            new UpdateUserProfileRequest("Auditory", ["JobSearcher"], "Medicine"));
 
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }
 
    [Fact]
    public async Task UpdateUserProfile_ThenGet_ReflectsNewValues()
    {
        var (authClient, _, _) = await CreateAndAuthenticateUserAsync();
        var id = await CreateProfile(authClient, new CreateUserProfileRequest("Visual", ["Student"], string.Empty));
 
        await authClient.PutAsJsonAsync($"/user-profile/{id}",
            new UpdateUserProfileRequest("Auditory", ["HobbyLearner"], "Medicine"));
 
        var getResponse = await authClient.GetAsync($"/user-profile/{id}");
        var profile = await getResponse.Content.ReadFromJsonAsync<UserProfileDto>();
 
        profile.ShouldNotBeNull();
        profile.LearningType.ShouldBe("Auditory");
        profile.Domain.ShouldBe("Medicine");
    }
 
    [Fact]
    public async Task UpdateUserProfile_Unauthenticated_Returns401()
    {
        var (authClient, _, _) = await CreateAndAuthenticateUserAsync();
        var id = await CreateProfile(authClient, new CreateUserProfileRequest("Visual", ["Student"], string.Empty));
 
        var response = await AnonymousClient.PutAsJsonAsync($"/user-profile/{id}",
            new UpdateUserProfileRequest("Auditory", ["JobSearcher"], "Medicine"));
 
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
 
    [Fact]
    public async Task UpdateUserProfile_InvalidLearningType_Returns400()
    {
        var (authClient, _, _) = await CreateAndAuthenticateUserAsync();
        var id = await CreateProfile(authClient, new CreateUserProfileRequest("Visual", ["Student"], string.Empty));
 
        var response = await authClient.PutAsJsonAsync($"/user-profile/{id}",
            new UpdateUserProfileRequest("NotAType", ["Student"], "Student"));
 
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
 
    [Fact]
    public async Task DeleteUserProfile_ValidId_Returns204NoContent()
    {
        var (authClient, _, _) = await CreateAndAuthenticateUserAsync();
        var id = await CreateProfile(authClient, new CreateUserProfileRequest("Visual", ["Student"], string.Empty));
 
        var response = await authClient.DeleteAsync($"/user-profile/{id}");
 
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }
 
    [Fact]
    public async Task DeleteUserProfile_ThenGetById_Returns404()
    {
        var (authClient, _, _) = await CreateAndAuthenticateUserAsync();
        var id = await CreateProfile(authClient, new CreateUserProfileRequest("Visual", ["Student"], string.Empty));
 
        await authClient.DeleteAsync($"/user-profile/{id}");
 
        var getResponse = await authClient.GetAsync($"/user-profile/{id}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
 
    [Fact]
    public async Task DeleteUserProfile_Unauthenticated_Returns401()
    {
        var (authClient, _, _) = await CreateAndAuthenticateUserAsync();
        var id = await CreateProfile(authClient, new CreateUserProfileRequest("Visual", ["Student"], string.Empty));
 
        var response = await AnonymousClient.DeleteAsync($"/user-profile/{id}");
 
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
 
    [Theory]
    [InlineData("Visual")]
    [InlineData("Auditory")]
    [InlineData("ReadWrite")]
    [InlineData("Combined")]
    public async Task CreateUserProfile_ValidLearningType_Returns200(string learningType)
    {
        var (authClient, _, _) = await CreateAndAuthenticateUserAsync();
        var request = new CreateUserProfileRequest(learningType, ["Student"], string.Empty);
 
        var response = await authClient.PostAsJsonAsync("/user-profile", request);
 
        response.StatusCode.ShouldBe(HttpStatusCode.OK,
            $"Expected OK for learning type '{learningType}' but got {response.StatusCode}");
    }
 
    [Fact]
    public async Task GetUserProfiles_MultipleUsersWithProfiles_ReturnsAllProfiles()
    {
        var (client1, _, _) = await CreateAndAuthenticateUserAsync();
        var (client2, _, _) = await CreateAndAuthenticateUserAsync();
 
        await CreateProfile(client1, new CreateUserProfileRequest("Visual", ["Student"], string.Empty));
        await CreateProfile(client2, new CreateUserProfileRequest("Auditory", ["JobSearcher"], string.Empty));
 
        var response = await client1.GetAsync("/user-profile");
        var result = await response.Content.ReadFromJsonAsync<List<UserProfileDto>>();
 
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThanOrEqualTo(2);
    }
 
    private static async Task<Guid> CreateProfile(HttpClient client, CreateUserProfileRequest request)
    {
        var response = await client.PostAsJsonAsync("/user-profile", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>();
    }
}
 