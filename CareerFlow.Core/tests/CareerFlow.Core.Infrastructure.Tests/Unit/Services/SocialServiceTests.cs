using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Models.Authentication;
using CareerFlow.Core.Infrastructure.Configurations;
using CareerFlow.Core.Infrastructure.Services;
using CareerFlow.Core.Infrastructure.Tests.Unit.Setup;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Unit.Services;

public class SocialServiceTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly FakeCacheService _cacheService = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();

    private readonly SocialAuthSettings _settings = new()
    {
        BaseUrl = "https://api.careerflow.com",
        Google = new GoogleSettings { ClientId = "google-client-id",ClientSecret = "google-client-secret"},
        LinkedIn = new LinkedInSettings { ClientId = "linkedin-client-id",ClientSecret ="linkedin-client-secret",RedirectUri = "linkedin-redirect-uri"}
    };

    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private SocialService CreateSut()
    {
        return new SocialService(
            Options.Create(_settings),
            _authServiceMock.Object,
            _tokenServiceMock.Object,
            _refreshTokenRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _cacheService);
    }

    private static Account MakeAccount() => Account.Create("user@example.com", "P@ssw0rd!", "testuser", "Test User");

    private static AuthResult MakeJwt(string token = "jwt-token") => new(token, Guid.NewGuid().ToString());

    private static (RefreshToken Token, string ExpectedHash) MakeRefreshToken(
        string rawToken = "raw-refresh-token-value")
    {
        var token = RefreshToken.Create(
            Guid.NewGuid(),
            rawToken,
            Guid.NewGuid().ToString(),
            DateTime.UtcNow.AddMonths(6));

        string expectedHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

        return (token, expectedHash);
    }

    private static string ExtractStateFromGoogleUrl(string url)
    {
        NameValueCollection pairs = HttpUtility.ParseQueryString(new Uri(url).Query);
        return pairs["state"] ?? throw new InvalidOperationException("state not found");
    }

    private static string ExtractStateFromLinkedInUrl(string url)
    {
        NameValueCollection pairs = HttpUtility.ParseQueryString(new Uri(url).Query);
        return pairs["state"] ?? throw new InvalidOperationException("state not found");
    }

    [Fact]
    public async Task GoogleMobileLogin_ReturnsValidGoogleOAuthUrl()
    {
        SocialService sut = CreateSut();
        string url = await sut.GoogleMobileLogin();
        Assert.StartsWith("https://accounts.google.com/o/oauth2/v2/auth", url);
    }

    [Fact]
    public async Task GoogleMobileLogin_ContainsClientId()
    {
        SocialService sut = CreateSut();
        string url = await sut.GoogleMobileLogin();
        Assert.Contains($"client_id={_settings.Google.ClientId}", url);
    }

    [Fact]
    public async Task GoogleMobileLogin_ContainsEncodedRedirectUri()
    {
        SocialService sut = CreateSut();
        string expected = Uri.EscapeDataString($"{_settings.BaseUrl}/social/auth/google/mobile/callback");
        string url = await sut.GoogleMobileLogin();
        Assert.Contains($"redirect_uri={expected}", url);
    }

    [Fact]
    public async Task GoogleMobileLogin_ContainsStateParam()
    {
        SocialService sut = CreateSut();
        string url = await sut.GoogleMobileLogin();
        Assert.Contains("state=", url);
    }

    [Fact]
    public async Task GoogleMobileLogin_StoresReturnUrlInCache_WhenReturnUrlProvided()
    {
        SocialService sut = CreateSut();
        const string returnUrl = "exp://192.168.1.1:8081/--/auth";
        string state = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin(returnUrl));
        Assert.True(_cacheService.TryGetValue($"oauth_state:{state}", out string? cached));
        Assert.Equal(returnUrl, cached);
    }

    [Fact]
    public async Task GoogleMobileLogin_StoresDefaultCallbackUrl_WhenReturnUrlIsNull()
    {
        SocialService sut = CreateSut();
        string state = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin());
        Assert.True(_cacheService.TryGetValue($"oauth_state:{state}", out string? cached));
        Assert.Equal("careerflow://auth/callback", cached);
    }

    [Fact]
    public async Task GoogleMobileLogin_StoresDefaultCallbackUrl_WhenReturnUrlIsWhitespace()
    {
        SocialService sut = CreateSut();
        string state = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin("   "));
        Assert.True(_cacheService.TryGetValue($"oauth_state:{state}", out string? cached));
        Assert.Equal("careerflow://auth/callback", cached);
    }

    [Fact]
    public async Task GoogleMobileLogin_GeneratesUniqueStateOnEachCall()
    {
        SocialService sut = CreateSut();
        string state1 = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin());
        string state2 = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin());
        Assert.NotEqual(state1, state2);
    }

    [Fact]
    public async Task GoogleMobileCallBackAsync_ReturnsUrlContainingJwtToken()
    {
        SocialService sut = CreateSut();
        Account account = MakeAccount();
        AuthResult jwt = MakeJwt("my-jwt");
        (RefreshToken refresh, _) = MakeRefreshToken();
        string state = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin("exp://192.168.1.1:8081/--/auth"));
        SetupFullGoogleFlow(account, jwt, refresh);
        string result = await sut.GoogleMobileCallBackAsync("auth-code", state, CancellationToken.None);
        Assert.Contains("token=my-jwt", result);
    }

    [Fact]
    public async Task GoogleMobileCallBackAsync_ReturnsUrlContainingHashedRefreshToken()
    {
        SocialService sut = CreateSut();
        Account account = MakeAccount();
        AuthResult jwt = MakeJwt();
        (RefreshToken refresh, string expectedHash) = MakeRefreshToken("known-raw-token");
        string state = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin("exp://host/callback"));
        SetupFullGoogleFlow(account, jwt, refresh);
        string result = await sut.GoogleMobileCallBackAsync("code", state, CancellationToken.None);
        Assert.Contains($"refreshToken={expectedHash}", result);
    }

    [Fact]
    public async Task GoogleMobileCallBackAsync_AppendsTokensWithQuestionMark_WhenNoExistingQuery()
    {
        SocialService sut = CreateSut();
        Account account = MakeAccount();
        (RefreshToken refresh, _) = MakeRefreshToken();
        const string returnUrl = "exp://192.168.1.1:8081/--/auth";
        string state = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin(returnUrl));
        SetupFullGoogleFlow(account, MakeJwt(), refresh);
        string result = await sut.GoogleMobileCallBackAsync("code", state, CancellationToken.None);
        Assert.Contains("?token=", result);
    }

    [Fact]
    public async Task GoogleMobileCallBackAsync_AppendsTokensWithAmpersand_WhenQueryAlreadyExists()
    {
        SocialService sut = CreateSut();
        Account account = MakeAccount();
        (RefreshToken refresh, _) = MakeRefreshToken();
        const string returnUrl = "exp://192.168.1.1:8081/--/auth?source=social";
        string state = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin(returnUrl));
        SetupFullGoogleFlow(account, MakeJwt(), refresh);
        string result = await sut.GoogleMobileCallBackAsync("code", state, CancellationToken.None);
        Assert.Contains("&token=", result);
    }

    [Fact]
    public async Task GoogleMobileCallBackAsync_KeepsStateInCacheAfterUse()
    {
        SocialService sut = CreateSut();
        Account account = MakeAccount();
        (RefreshToken refresh, _) = MakeRefreshToken();
        SetupFullGoogleFlow(account, MakeJwt(), refresh);
        string state = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin("exp://host/callback"));
        await sut.GoogleMobileCallBackAsync("code", state, CancellationToken.None);
        Assert.True(_cacheService.TryGetValue<string>($"oauth_state:{state}", out _));
    }

    [Fact]
    public async Task GoogleMobileCallBackAsync_SavesRefreshTokenAndCommits()
    {
        SocialService sut = CreateSut();
        Account account = MakeAccount();
        (RefreshToken refresh, _) = MakeRefreshToken();
        SetupFullGoogleFlow(account, MakeJwt(), refresh);
        string state = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin());
        await sut.GoogleMobileCallBackAsync("code", state, CancellationToken.None);
        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(refresh, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GoogleMobileCallBackAsync_ReturnsErrorUrl_WhenStateIsInvalid()
    {
        SocialService sut = CreateSut();
        string result = await sut.GoogleMobileCallBackAsync("code", "invalid-state", CancellationToken.None);
        Assert.Equal("careerflow://auth/callback?error=session_expired", result);
    }

    [Fact]
    public async Task GoogleMobileCallBackAsync_ReturnsErrorUrl_WhenStateIsEmpty()
    {
        SocialService sut = CreateSut();
        string result = await sut.GoogleMobileCallBackAsync("code", string.Empty, CancellationToken.None);
        Assert.Equal("careerflow://auth/callback?error=session_expired", result);
    }

    [Fact]
    public async Task GoogleMobileCallBackAsync_ReturnsDuplicateRequestUrl_WhenAuthThrows()
    {
        SocialService sut = CreateSut();
        string state = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin());
        _authServiceMock
            .Setup(s => s.ExchangeGoogleCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Mocked auth failure"));
        string result = await sut.GoogleMobileCallBackAsync("code", state, CancellationToken.None);
        Assert.Equal("careerflow://auth/callback?error=duplicate_request", result);
    }

    [Fact]
    public async Task LinkedInMobileLogin_ReturnsValidLinkedInOAuthUrl()
    {
        SocialService sut = CreateSut();
        string url = await sut.LinkedInMobileLogin();
        Assert.StartsWith("https://www.linkedin.com/oauth/v2/authorization", url);
    }

    [Fact]
    public async Task LinkedInMobileLogin_ContainsClientId()
    {
        SocialService sut = CreateSut();
        string url = await sut.LinkedInMobileLogin();
        Assert.Contains($"client_id={_settings.LinkedIn.ClientId}", url);
    }

    [Fact]
    public async Task LinkedInMobileLogin_ContainsEncodedRedirectUri()
    {
        SocialService sut = CreateSut();
        string expected = Uri.EscapeDataString($"{_settings.BaseUrl}/social/auth/linkedin/mobile/callback");
        string url = await sut.LinkedInMobileLogin();
        Assert.Contains($"redirect_uri={expected}", url);
    }

    [Fact]
    public async Task LinkedInMobileLogin_StoresReturnUrlInCache_WhenProvided()
    {
        SocialService sut = CreateSut();
        const string returnUrl = "exp://192.168.1.1:8081/--/auth";
        string state = ExtractStateFromLinkedInUrl(await sut.LinkedInMobileLogin(returnUrl));
        Assert.True(_cacheService.TryGetValue($"oauth_state:{state}", out string? cached));
        Assert.Equal(returnUrl, cached);
    }

    [Fact]
    public async Task LinkedInMobileLogin_StoresDefaultCallback_WhenReturnUrlIsNull()
    {
        SocialService sut = CreateSut();
        string state = ExtractStateFromLinkedInUrl(await sut.LinkedInMobileLogin());
        Assert.True(_cacheService.TryGetValue($"oauth_state:{state}", out string? cached));
        Assert.Equal("careerflow://auth/callback", cached);
    }

    [Fact]
    public async Task LinkedInMobileLogin_GeneratesUniqueStateOnEachCall()
    {
        SocialService sut = CreateSut();
        string state1 = ExtractStateFromLinkedInUrl(await sut.LinkedInMobileLogin());
        string state2 = ExtractStateFromLinkedInUrl(await sut.LinkedInMobileLogin());
        Assert.NotEqual(state1, state2);
    }

    [Fact]
    public async Task LinkedInCallBackAsync_ReturnsUrlContainingJwtToken()
    {
        SocialService sut = CreateSut();
        Account account = MakeAccount();
        AuthResult jwt = MakeJwt("linkedin-jwt");
        (RefreshToken refresh, _) = MakeRefreshToken();
        string state = ExtractStateFromLinkedInUrl(await sut.LinkedInMobileLogin("exp://host/callback"));
        SetupFullLinkedInFlow(account, jwt, refresh);
        string result = await sut.LinkedInCallBackAsync("li-code", state, CancellationToken.None);
        Assert.Contains("token=linkedin-jwt", result);
    }

    [Fact]
    public async Task LinkedInCallBackAsync_ReturnsUrlContainingHashedRefreshToken()
    {
        SocialService sut = CreateSut();
        Account account = MakeAccount();
        AuthResult jwt = MakeJwt();
        (RefreshToken refresh, string expectedHash) = MakeRefreshToken("known-linkedin-raw");
        string state = ExtractStateFromLinkedInUrl(await sut.LinkedInMobileLogin("exp://host/callback"));
        SetupFullLinkedInFlow(account, jwt, refresh);
        string result = await sut.LinkedInCallBackAsync("code", state, CancellationToken.None);
        Assert.Contains($"refreshToken={expectedHash}", result);
    }

    [Fact]
    public async Task LinkedInCallBackAsync_KeepsStateInCacheAfterUse()
    {
        SocialService sut = CreateSut();
        Account account = MakeAccount();
        (RefreshToken refresh, _) = MakeRefreshToken();
        SetupFullLinkedInFlow(account, MakeJwt(), refresh);
        string state = ExtractStateFromLinkedInUrl(await sut.LinkedInMobileLogin());
        await sut.LinkedInCallBackAsync("code", state, CancellationToken.None);
        Assert.True(_cacheService.TryGetValue<string>($"oauth_state:{state}", out _));
    }

    [Fact]
    public async Task LinkedInCallBackAsync_SavesRefreshTokenAndCommits()
    {
        SocialService sut = CreateSut();
        Account account = MakeAccount();
        (RefreshToken refresh, _) = MakeRefreshToken();
        SetupFullLinkedInFlow(account, MakeJwt(), refresh);
        string state = ExtractStateFromLinkedInUrl(await sut.LinkedInMobileLogin());
        await sut.LinkedInCallBackAsync("code", state, CancellationToken.None);
        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(refresh, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LinkedInCallBackAsync_ReturnsErrorUrl_WhenStateIsInvalid()
    {
        SocialService sut = CreateSut();
        string result = await sut.LinkedInCallBackAsync("code", "tampered-state", CancellationToken.None);
        Assert.Equal("careerflow://auth/callback?error=session_expired", result);
    }

    [Fact]
    public async Task LinkedInCallBackAsync_ReturnsDuplicateRequestUrl_WhenAuthThrows()
    {
        SocialService sut = CreateSut();
        string state = ExtractStateFromLinkedInUrl(await sut.LinkedInMobileLogin());
        _authServiceMock
            .Setup(s => s.LoginWithLinkedInAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Mocked auth failure"));
        string result = await sut.LinkedInCallBackAsync("code", state, CancellationToken.None);
        Assert.Equal("careerflow://auth/callback?error=duplicate_request", result);
    }

    [Fact]
    public async Task CrossProvider_UsingGoogleStateForLinkedIn_WhenAuthThrows_ReturnsDuplicateRequest()
    {
        SocialService sut = CreateSut();
        string state = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin());
        _authServiceMock
            .Setup(s => s.LoginWithLinkedInAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Invalid code for LinkedIn"));
        string result = await sut.LinkedInCallBackAsync("code", state, CancellationToken.None);
        Assert.Equal("careerflow://auth/callback?error=duplicate_request", result);
    }

    [Fact]
    public async Task TwoSimultaneousFlows_HaveIsolatedCacheEntries()
    {
        SocialService sut = CreateSut();
        const string returnUrl1 = "exp://device1/callback";
        const string returnUrl2 = "exp://device2/callback";
        string googleState = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin(returnUrl1));
        string linkedInState = ExtractStateFromLinkedInUrl(await sut.LinkedInMobileLogin(returnUrl2));
        Assert.NotEqual(googleState, linkedInState);
        Assert.True(_cacheService.TryGetValue($"oauth_state:{googleState}", out string? g) && g == returnUrl1);
        Assert.True(_cacheService.TryGetValue($"oauth_state:{linkedInState}", out string? l) && l == returnUrl2);
    }

    private void SetupFullGoogleFlow(Account account, AuthResult jwt, RefreshToken refresh)
    {
        _authServiceMock
            .Setup(s => s.ExchangeGoogleCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("id-token");
        _authServiceMock
            .Setup(s => s.LoginWithGoogleAsync("id-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _tokenServiceMock
            .Setup(s => s.GenerateToken(account))
            .Returns(jwt);
        _tokenServiceMock
            .Setup(s => s.GenerateRefreshToken(account.Id, jwt.Token))
            .Returns(refresh);
    }

    private void SetupFullLinkedInFlow(Account account, AuthResult jwt, RefreshToken refresh)
    {
        _authServiceMock
            .Setup(s => s.LoginWithLinkedInAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _tokenServiceMock
            .Setup(s => s.GenerateToken(account))
            .Returns(jwt);
        _tokenServiceMock
            .Setup(s => s.GenerateRefreshToken(account.Id, jwt.Token))
            .Returns(refresh);
    }
}
