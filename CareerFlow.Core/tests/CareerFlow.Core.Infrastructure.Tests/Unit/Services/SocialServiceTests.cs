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
        Google = new GoogleSettings { ClientId = "google-client-id" },
        LinkedIn = new LinkedInSettings { ClientId = "linkedin-client-id" }
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

    private static Account MakeAccount()
    {
        return Account.Create("user@example.com", "P@ssw0rd!", "testuser", "Test User");
    }

    private static AuthResult MakeJwt(string token = "jwt-token")
    {
        return new AuthResult(token, Guid.NewGuid().ToString());
    }

    private static (RefreshToken Token, string ExpectedHash) MakeRefreshToken(
        string rawToken = "raw-refresh-token-value")
    {
        var token = RefreshToken.Create(
            Guid.NewGuid(),
            rawToken,
            Guid.NewGuid().ToString(),
            DateTime.UtcNow.AddMonths(6));

        var expectedHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

        return (token, expectedHash);
    }

    private static string ExtractStateFromGoogleUrl(string url)
    {
        var pairs = HttpUtility.ParseQueryString(new Uri(url).Query);
        return pairs["state"]
               ?? throw new InvalidOperationException("state not found in Google URL");
    }

    private static string ExtractStateFromLinkedInUrl(string url)
    {
        var pairs = HttpUtility.ParseQueryString(new Uri(url).Query);
        return pairs["state"]
               ?? throw new InvalidOperationException("state not found in LinkedIn URL");
    }

    // ─── GoogleMobileLogin ────────────────────────────────────────────────────

    [Fact]
    public async Task GoogleMobileLogin_ReturnsValidGoogleOAuthUrl()
    {
        var sut = CreateSut();

        var url = await sut.GoogleMobileLogin();

        Assert.StartsWith("https://accounts.google.com/o/oauth2/v2/auth", url);
    }

    [Fact]
    public async Task GoogleMobileLogin_ContainsClientId()
    {
        var sut = CreateSut();

        var url = await sut.GoogleMobileLogin();

        Assert.Contains($"client_id={_settings.Google.ClientId}", url);
    }

    [Fact]
    public async Task GoogleMobileLogin_ContainsEncodedRedirectUri()
    {
        var sut = CreateSut();
        var expected = Uri.EscapeDataString($"{_settings.BaseUrl}/social/auth/google/mobile/callback");

        var url = await sut.GoogleMobileLogin();

        Assert.Contains($"redirect_uri={expected}", url);
    }

    [Fact]
    public async Task GoogleMobileLogin_ContainsStateParam()
    {
        var sut = CreateSut();

        var url = await sut.GoogleMobileLogin();

        Assert.Contains("state=", url);
    }

    [Fact]
    public async Task GoogleMobileLogin_StoresReturnUrlInCache_WhenReturnUrlProvided()
    {
        var sut = CreateSut();
        const string returnUrl = "exp://192.168.1.1:8081/--/auth";

        var state = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin(returnUrl));

        Assert.True(_cacheService.TryGetValue<string>($"oauth_state:{state}", out var cached));
        Assert.Equal(returnUrl, cached);
    }

    [Fact]
    public async Task GoogleMobileLogin_StoresDefaultCallbackUrl_WhenReturnUrlIsNull()
    {
        var sut = CreateSut();

        var state = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin());

        Assert.True(_cacheService.TryGetValue<string>($"oauth_state:{state}", out var cached));
        Assert.Equal("careerflow://auth/callback", cached);
    }

    [Fact]
    public async Task GoogleMobileLogin_StoresDefaultCallbackUrl_WhenReturnUrlIsWhitespace()
    {
        var sut = CreateSut();

        var state = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin("   "));

        Assert.True(_cacheService.TryGetValue<string>($"oauth_state:{state}", out var cached));
        Assert.Equal("careerflow://auth/callback", cached);
    }

    [Fact]
    public async Task GoogleMobileLogin_GeneratesUniqueStateOnEachCall()
    {
        var sut = CreateSut();

        var state1 = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin());
        var state2 = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin());

        Assert.NotEqual(state1, state2);
    }

    // ─── GoogleMobileCallBackAsync ────────────────────────────────────────────

    [Fact]
    public async Task GoogleMobileCallBackAsync_ReturnsUrlContainingJwtToken()
    {
        var sut = CreateSut();
        var account = MakeAccount();
        var jwt = MakeJwt("my-jwt");
        var (refresh, _) = MakeRefreshToken();

        var state = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin("exp://192.168.1.1:8081/--/auth"));
        SetupFullGoogleFlow(account, jwt, refresh);

        var result = await sut.GoogleMobileCallBackAsync("auth-code", state, CancellationToken.None);

        Assert.Contains("token=my-jwt", result);
    }

    [Fact]
    public async Task GoogleMobileCallBackAsync_ReturnsUrlContainingHashedRefreshToken()
    {
        var sut = CreateSut();
        var account = MakeAccount();
        var jwt = MakeJwt();
        var (refresh, expectedHash) = MakeRefreshToken("known-raw-token");

        var state = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin("exp://host/callback"));
        SetupFullGoogleFlow(account, jwt, refresh);

        var result = await sut.GoogleMobileCallBackAsync("code", state, CancellationToken.None);

        Assert.Contains($"refreshToken={expectedHash}", result);
    }

    [Fact]
    public async Task GoogleMobileCallBackAsync_AppendsTokensWithQuestionMark_WhenNoExistingQuery()
    {
        var sut = CreateSut();
        var account = MakeAccount();
        var (refresh, _) = MakeRefreshToken();

        const string returnUrl = "exp://192.168.1.1:8081/--/auth";
        var state = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin(returnUrl));
        SetupFullGoogleFlow(account, MakeJwt(), refresh);

        var result = await sut.GoogleMobileCallBackAsync("code", state, CancellationToken.None);

        Assert.Contains("?token=", result);
    }

    [Fact]
    public async Task GoogleMobileCallBackAsync_AppendsTokensWithAmpersand_WhenQueryAlreadyExists()
    {
        var sut = CreateSut();
        var account = MakeAccount();
        var (refresh, _) = MakeRefreshToken();

        const string returnUrl = "exp://192.168.1.1:8081/--/auth?source=social";
        var state = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin(returnUrl));
        SetupFullGoogleFlow(account, MakeJwt(), refresh);

        var result = await sut.GoogleMobileCallBackAsync("code", state, CancellationToken.None);

        Assert.Contains("&token=", result);
    }

    [Fact]
    public async Task GoogleMobileCallBackAsync_RemovesStateFromCacheAfterUse()
    {
        var sut = CreateSut();
        var account = MakeAccount();
        var (refresh, _) = MakeRefreshToken();
        SetupFullGoogleFlow(account, MakeJwt(), refresh);

        var state = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin("exp://host/callback"));
        await sut.GoogleMobileCallBackAsync("code", state, CancellationToken.None);

        Assert.False(_cacheService.TryGetValue<string>($"oauth_state:{state}", out _));
    }

    [Fact]
    public async Task GoogleMobileCallBackAsync_SavesRefreshTokenAndCommits()
    {
        var sut = CreateSut();
        var account = MakeAccount();
        var (refresh, _) = MakeRefreshToken();
        SetupFullGoogleFlow(account, MakeJwt(), refresh);

        var state = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin());
        await sut.GoogleMobileCallBackAsync("code", state, CancellationToken.None);

        _refreshTokenRepositoryMock.Verify(
            r => r.AddAsync(refresh, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GoogleMobileCallBackAsync_ThrowsInvalidOperationException_WhenStateIsInvalid()
    {
        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.GoogleMobileCallBackAsync("code", "invalid-state", CancellationToken.None));
    }

    [Fact]
    public async Task GoogleMobileCallBackAsync_ThrowsInvalidOperationException_WhenStateIsEmpty()
    {
        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.GoogleMobileCallBackAsync("code", string.Empty, CancellationToken.None));
    }

    [Fact]
    public async Task GoogleMobileCallBackAsync_ThrowsInvalidOperationException_WhenStateIsReused()
    {
        var sut = CreateSut();
        var account = MakeAccount();
        var (refresh, _) = MakeRefreshToken();
        SetupFullGoogleFlow(account, MakeJwt(), refresh);

        var state = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin());
        await sut.GoogleMobileCallBackAsync("code", state, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.GoogleMobileCallBackAsync("code", state, CancellationToken.None));
    }

    // ─── LinkedInMobileLogin ──────────────────────────────────────────────────

    [Fact]
    public async Task LinkedInMobileLogin_ReturnsValidLinkedInOAuthUrl()
    {
        var sut = CreateSut();

        var url = await sut.LinkedInMobileLogin();

        Assert.StartsWith("https://www.linkedin.com/oauth/v2/authorization", url);
    }

    [Fact]
    public async Task LinkedInMobileLogin_ContainsClientId()
    {
        var sut = CreateSut();

        var url = await sut.LinkedInMobileLogin();

        Assert.Contains($"client_id={_settings.LinkedIn.ClientId}", url);
    }

    [Fact]
    public async Task LinkedInMobileLogin_ContainsEncodedRedirectUri()
    {
        var sut = CreateSut();
        var expected = Uri.EscapeDataString($"{_settings.BaseUrl}/social/auth/linkedin/mobile/callback");

        var url = await sut.LinkedInMobileLogin();

        Assert.Contains($"redirect_uri={expected}", url);
    }

    [Fact]
    public async Task LinkedInMobileLogin_StoresReturnUrlInCache_WhenProvided()
    {
        var sut = CreateSut();
        const string returnUrl = "exp://192.168.1.1:8081/--/auth";

        var state = ExtractStateFromLinkedInUrl(await sut.LinkedInMobileLogin(returnUrl));

        Assert.True(_cacheService.TryGetValue<string>($"oauth_state:{state}", out var cached));
        Assert.Equal(returnUrl, cached);
    }

    [Fact]
    public async Task LinkedInMobileLogin_StoresDefaultCallback_WhenReturnUrlIsNull()
    {
        var sut = CreateSut();

        var state = ExtractStateFromLinkedInUrl(await sut.LinkedInMobileLogin());

        Assert.True(_cacheService.TryGetValue<string>($"oauth_state:{state}", out var cached));
        Assert.Equal("careerflow://auth/callback", cached);
    }

    [Fact]
    public async Task LinkedInMobileLogin_GeneratesUniqueStateOnEachCall()
    {
        var sut = CreateSut();

        var state1 = ExtractStateFromLinkedInUrl(await sut.LinkedInMobileLogin());
        var state2 = ExtractStateFromLinkedInUrl(await sut.LinkedInMobileLogin());

        Assert.NotEqual(state1, state2);
    }

    // ─── LinkedInCallBackAsync ────────────────────────────────────────────────

    [Fact]
    public async Task LinkedInCallBackAsync_ReturnsUrlContainingJwtToken()
    {
        var sut = CreateSut();
        var account = MakeAccount();
        var jwt = MakeJwt("linkedin-jwt");
        var (refresh, _) = MakeRefreshToken();

        var state = ExtractStateFromLinkedInUrl(await sut.LinkedInMobileLogin("exp://host/callback"));
        SetupFullLinkedInFlow(account, jwt, refresh);

        var result = await sut.LinkedInCallBackAsync("li-code", state, CancellationToken.None);

        Assert.Contains("token=linkedin-jwt", result);
    }

    [Fact]
    public async Task LinkedInCallBackAsync_ReturnsUrlContainingHashedRefreshToken()
    {
        var sut = CreateSut();
        var account = MakeAccount();
        var jwt = MakeJwt();
        var (refresh, expectedHash) = MakeRefreshToken("known-linkedin-raw");

        var state = ExtractStateFromLinkedInUrl(await sut.LinkedInMobileLogin("exp://host/callback"));
        SetupFullLinkedInFlow(account, jwt, refresh);

        var result = await sut.LinkedInCallBackAsync("code", state, CancellationToken.None);

        Assert.Contains($"refreshToken={expectedHash}", result);
    }

    [Fact]
    public async Task LinkedInCallBackAsync_RemovesStateFromCacheAfterUse()
    {
        var sut = CreateSut();
        var account = MakeAccount();
        var (refresh, _) = MakeRefreshToken();
        SetupFullLinkedInFlow(account, MakeJwt(), refresh);

        var state = ExtractStateFromLinkedInUrl(await sut.LinkedInMobileLogin());
        await sut.LinkedInCallBackAsync("code", state, CancellationToken.None);

        Assert.False(_cacheService.TryGetValue<string>($"oauth_state:{state}", out _));
    }

    [Fact]
    public async Task LinkedInCallBackAsync_SavesRefreshTokenAndCommits()
    {
        var sut = CreateSut();
        var account = MakeAccount();
        var (refresh, _) = MakeRefreshToken();
        SetupFullLinkedInFlow(account, MakeJwt(), refresh);

        var state = ExtractStateFromLinkedInUrl(await sut.LinkedInMobileLogin());
        await sut.LinkedInCallBackAsync("code", state, CancellationToken.None);

        _refreshTokenRepositoryMock.Verify(
            r => r.AddAsync(refresh, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LinkedInCallBackAsync_ThrowsInvalidOperationException_WhenStateIsInvalid()
    {
        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.LinkedInCallBackAsync("code", "tampered-state", CancellationToken.None));
    }

    [Fact]
    public async Task LinkedInCallBackAsync_ThrowsInvalidOperationException_WhenStateIsReused()
    {
        var sut = CreateSut();
        var account = MakeAccount();
        var (refresh, _) = MakeRefreshToken();
        SetupFullLinkedInFlow(account, MakeJwt(), refresh);

        var state = ExtractStateFromLinkedInUrl(await sut.LinkedInMobileLogin());
        await sut.LinkedInCallBackAsync("code", state, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.LinkedInCallBackAsync("code", state, CancellationToken.None));
    }

    // ─── Cross-provider isolation ─────────────────────────────────────────────

    [Fact]
    public async Task GoogleState_CannotBeReused_AfterGoogleCallbackConsumesIt()
    {
        var sut = CreateSut();
        var account = MakeAccount();
        var (refresh, _) = MakeRefreshToken();
        SetupFullGoogleFlow(account, MakeJwt(), refresh);

        var state = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin());
        await sut.GoogleMobileCallBackAsync("code", state, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.LinkedInCallBackAsync("code", state, CancellationToken.None));
    }

    [Fact]
    public async Task TwoSimultaneousFlows_HaveIsolatedCacheEntries()
    {
        var sut = CreateSut();

        const string returnUrl1 = "exp://device1/callback";
        const string returnUrl2 = "exp://device2/callback";

        var googleState = ExtractStateFromGoogleUrl(await sut.GoogleMobileLogin(returnUrl1));
        var linkedInState = ExtractStateFromLinkedInUrl(await sut.LinkedInMobileLogin(returnUrl2));

        Assert.NotEqual(googleState, linkedInState);
        Assert.True(_cacheService.TryGetValue<string>($"oauth_state:{googleState}", out var g) && g == returnUrl1);
        Assert.True(_cacheService.TryGetValue<string>($"oauth_state:{linkedInState}", out var l) && l == returnUrl2);
    }

    // ─── Private setup helpers ────────────────────────────────────────────────

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