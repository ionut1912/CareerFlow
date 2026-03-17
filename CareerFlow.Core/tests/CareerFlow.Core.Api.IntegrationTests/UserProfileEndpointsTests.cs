using System.Net;
using System.Net.Http.Json;
using CareerFlow.Core.Api.IntegrationTests.Setup;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Requests;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Api.IntegrationTests;

[Trait("Category", "Integration")]
public class UserProfileEndpointsTests : IntegrationTestBase
{
    public UserProfileEndpointsTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateUserProfile_ValidRequest_CreatesUserProfile()
    {
        // Arrange
        var (authClient, account, _) = await CreateAndAuthenticateUserAsync();
        var request = new CreateUserProfileRequest("Visual", ["Student"], string.Empty);

        //Act
        var response = await authClient.PostAsJsonAsync("/user-profile", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Guid>();

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBe(account.Id);
    }

    [Fact]
    public async Task CreateUserProfile_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var (authClient, _, _) = await CreateAndAuthenticateUserAsync();
        var request = new CreateUserProfileRequest("Visasdsa", ["Student"], string.Empty);

        //Act
        var response = await authClient.PostAsJsonAsync("/user-profile", request);

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUserProfile_UnauthenticatedUser_ReturnsUnauthorized()
    {
        //Arrange
        var request = new CreateUserProfileRequest("Visasdsa", ["Student"], string.Empty);

        //Act
        var response = await AnonymousClient.PostAsJsonAsync("/user-profile", request);

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserProfiles_AuthenticatedUser_ReturnsUserProfiles()
    {
        // Arrange
        var (authClient, account, _) = await CreateAndAuthenticateUserAsync();
        var createUserProfileRequest = new CreateUserProfileRequest("Visual", ["Student"], string.Empty);
        var id = await CreateUserProfile(authClient, createUserProfileRequest);

        //Act
        var response = await authClient.GetAsync("/user-profile");

        //Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<UserProfileDto>>();
        result.Count.ShouldBeGreaterThan(0);
        result[0].Id.ShouldBe(id);
        result[0].LearningType.ShouldBe(createUserProfileRequest.LearningType);
        result[0].UserTypes.ShouldBe(createUserProfileRequest.UserTypes);
        result[0].Domain.ShouldBe(createUserProfileRequest.Domain);
        result[0].AccountId.ShouldBe(account.Id);
    }

    [Fact]
    public async Task GetUserProfiles_UnauthenticatedUser_ReturnsUnauthorized()
    {
        //Act
        var response = await AnonymousClient.GetAsync("/user-profile");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserProfile_AuthenticatedUser_ReturnsUserProfile()
    {
        // Arrange
        var (authClient, account, _) = await CreateAndAuthenticateUserAsync();

        var request = new CreateUserProfileRequest("Visual", ["Student"], string.Empty);
        var id = await CreateUserProfile(authClient, request);


        //Act
        var response = await authClient.GetAsync($"/user-profile/{id}");

        //Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UserProfileDto>();
        result.Id.ShouldBe(id);
        result.LearningType.ShouldBe(request.LearningType);
        result.UserTypes.ShouldBe(request.UserTypes);
        result.Domain.ShouldBe(request.Domain);
        result.AccountId.ShouldBe(account.Id);
    }

    [Fact]
    public async Task GetUserProfile_DifferentId_ReturnsUserProfile()
    {
        // Arrange
        var (authClient, _, _) = await CreateAndAuthenticateUserAsync();

        //Act
        var response = await authClient.GetAsync($"/user-profile/{Guid.NewGuid()}");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserProfile_UnauthenticatedUser_ReturnsUnauthorized()
    {
        //Act
        var response = await AnonymousClient.GetAsync($"/user-profile/{Guid.NewGuid()}");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUserProfile_AuthenticatedUser_ReturnsCurrentUserProfile()
    {
        // Arrange
        var (authClient, account, _) = await CreateAndAuthenticateUserAsync();

        var request = new CreateUserProfileRequest("Visual", ["Student"], string.Empty);
        var id = await CreateUserProfile(authClient, request);

        //Act
        var response = await authClient.GetAsync("/user-profile/current");

        //Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UserProfileDto>();
        result.Id.ShouldBe(id);
        result.LearningType.ShouldBe(request.LearningType);
        result.UserTypes.ShouldBe(request.UserTypes);
        result.Domain.ShouldBe(request.Domain);
        result.AccountId.ShouldBe(account.Id);
    }

    [Fact]
    public async Task GetCurrentUserProfile_UnauthenticatedUser_ReturnsUnauthorized()
    {
        //Act
        var response = await AnonymousClient.GetAsync("/user-profile/current");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateUserProfile_AuthenticatedUser_UpdateProfile()
    {
        // Arrange
        var (authClient, _, _) = await CreateAndAuthenticateUserAsync();

        var request = new CreateUserProfileRequest("Visual", ["Student"], string.Empty);
        var id = await CreateUserProfile(authClient, request);
        var updateRequest = new UpdateUserProfileRequest("Auditory", ["JobSearcher", "HobbyLearner"], "Medicine");

        //Act
        var updateResponse = await authClient.PutAsJsonAsync($"/user-profile/{id}", updateRequest);

        //Assert
        updateResponse.EnsureSuccessStatusCode();
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateUserProfile_UnauthenticatedUser_ReturnsUnauthorized()
    {
        //Arrange
        var (authClient, _, _) = await CreateAndAuthenticateUserAsync();
        var request = new CreateUserProfileRequest("Visual", ["Student"], string.Empty);
        var id = await CreateUserProfile(authClient, request);
        var updateRequest = new UpdateUserProfileRequest("Auditory", ["JobSearcher", "HobbyLearner"], "Medicine");

        //Act
        var updateResponse = await AnonymousClient.PutAsJsonAsync($"/user-profile/{id}", updateRequest);

        //Assert
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteUserProfile_AuthenticatedUser_DeleteProfile()
    {
        //Arrange
        var (authClient, _, _) = await CreateAndAuthenticateUserAsync();
        var request = new CreateUserProfileRequest("Visual", ["Student"], string.Empty);
        var id = await CreateUserProfile(authClient, request);

        //Act
        var deleteRequest = await authClient.DeleteAsync($"/user-profile/{id}");

        //Assert
        deleteRequest.EnsureSuccessStatusCode();
        deleteRequest.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteUserProfile_UnauthenticatedUser_ReturnsUnauthorized()
    {
        //Arrange
        var (authClient, _, _) = await CreateAndAuthenticateUserAsync();
        var request = new CreateUserProfileRequest("Visual", ["Student"], string.Empty);
        var id = await CreateUserProfile(authClient, request);

        //Act
        var deleteRequest = await AnonymousClient.DeleteAsync($"/user-profile/{id}");

        //Assert
        deleteRequest.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private static async Task<Guid> CreateUserProfile(HttpClient authClient, CreateUserProfileRequest request)
    {
        var createResponse = await authClient.PostAsJsonAsync("/user-profile", request);
        createResponse.EnsureSuccessStatusCode();
        return await createResponse.Content.ReadFromJsonAsync<Guid>();
    }
}