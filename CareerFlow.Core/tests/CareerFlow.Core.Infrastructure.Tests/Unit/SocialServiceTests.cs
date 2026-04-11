using System.Security.Cryptography;
using System.Text;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Models.Authentication;
using CareerFlow.Core.Infrastructure.Configurations;
using CareerFlow.Core.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Unit;

public class SocialServiceTests
{
    // ─── Fixtures ────────────────────────────────────────────────────────────

    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());

    // Fix 1: Use the correct nested settings class names (GoogleSettings / LinkedInSettings)
    private readonly SocialAuthSettings _settings = new()
    {
        BaseUrl  = "https://api.careerflow.com",
        Google   = new GoogleSettings   { ClientId = "google-client-id" },
        LinkedIn = new LinkedInSettings { ClientId = "linkedin-client-id" }
    };

    private SocialService CreateSut() => new(
        Options.Create(_settings),
        _authServiceMock.Object,
        _tokenServiceMock.Object,
        _refreshTokenRepositoryMock.Object,
        _unitOfWorkMock.Object,
        _memoryCache);

    // Fix 2: Account has no public constructor — use the static factory method.
    private static Account MakeAccount() =>
        Account.Create("user@example.com", "P@ssw0rd!", "testuser", "Test User");

    // Helper: builds a dummy AuthResult (token + jti)
    private static AuthResult MakeJwt(string token = "jwt-token") =>
        new(token, Guid.NewGuid().ToString());

    // Fix 3: RefreshToken.TokenHash is SHA-256 of the raw token, not the raw value itself.
    // We create a real RefreshToken via its factory and expose the *expected* hash so
    // tests can assert the correct value without reimplementing the hashing logic.
    private static (RefreshToken Token, string ExpectedHash) MakeRefreshToken(
        string rawToken = "raw-refresh-token-value")
    {
        var token = RefreshToken.Create(
            Guid.NewGuid(),
            rawToken,
            Guid.NewGuid().ToString(),
            DateTime.UtcNow.AddMonths(6));

        // Mirror the private HashToken logic from RefreshToken so assertions stay honest.
        var expectedHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

        return (token, expectedHash);
    }

    // Helper: extract ?state= from a Google OAuth URL
    private static string ExtractStateFromGoogleUrl(string url)
    {
        var pairs = System.Web.HttpUtility.ParseQueryString(new Uri(url).Query);
        return pairs["state"]
               ?? throw new InvalidOperationException("state not found in Google URL");
    }

    // Helper: extract ?state= from a LinkedIn OAuth URL
    private static string ExtractStateFromLinkedInUrl(string url)
    {
        var pairs = System.Web.HttpUtility.ParseQueryString(new Uri(url).Query);
        return pairs["state"]
               ?? throw new InvalidOperationException("state not found in LinkedIn URL");
    }

    // ─── GoogleMobileLogin ────────────────────────────────────────────────────

    [Fact]
    public void GoogleMobileLogin_ReturnsValidGoogleOAuthUrl()
    {
        var sut = CreateSut();

        var url = sut.GoogleMobileLogin();

        Assert.StartsWith("https://accounts.google.com/o/oauth2/v2/auth", url);
    }

    [Fact]
    public void GoogleMobileLogin_ContainsClientId()
    {
        var sut = CreateSut();

        var url = sut.GoogleMobileLogin();

        Assert.Contains($"client_id={_settings.Google.ClientId}", url);
    }

    [Fact]
    public void GoogleMobileLogin_ContainsEncodedRedirectUri()
    {
        var sut = CreateSut();
        var expected = Uri.EscapeDataString($"{_settings.BaseUrl}/social/auth/google/mobile/callback");

        var url = sut.GoogleMobileLogin();

        Assert.Contains($"redirect_uri={expected}", url);
    }

    [Fact]
    public void GoogleMobileLogin_ContainsStateParam()
    {
        var sut = CreateSut();

        var url = sut.GoogleMobileLogin();

        Assert.Contains("state=", url);
    }

    [Fact]
    public void GoogleMobileLogin_StoresReturnUrlInCache_WhenReturnUrlProvided()
    {
        var sut = CreateSut();
        const string returnUrl = "exp://192.168.1.1:8081/--/auth";

        var state = ExtractStateFromGoogleUrl(sut.GoogleMobileLogin(returnUrl));

        Assert.True(_memoryCache.TryGetValue(state, out string? cached));
        Assert.Equal(returnUrl, cached);
    }

    [Fact]
    public void GoogleMobileLogin_StoresDefaultCallbackUrl_WhenReturnUrlIsNull()
    {
        var sut = CreateSut();

        var state = ExtractStateFromGoogleUrl(sut.GoogleMobileLogin(null));

        Assert.True(_memoryCache.TryGetValue(state, out string? cached));
        Assert.Equal("careerflow://auth/callback", cached);
    }

    [Fact]
    public void GoogleMobileLogin_StoresDefaultCallbackUrl_WhenReturnUrlIsWhitespace()
    {
        var sut = CreateSut();

        var state = ExtractStateFromGoogleUrl(sut.GoogleMobileLogin("   "));

        Assert.True(_memoryCache.TryGetValue(state, out string? cached));
        Assert.Equal("careerflow://auth/callback", cached);
    }

    [Fact]
    public void GoogleMobileLogin_GeneratesUniqueStateOnEachCall()
    {
        var sut = CreateSut();

        var state1 = ExtractStateFromGoogleUrl(sut.GoogleMobileLogin());
        var state2 = ExtractStateFromGoogleUrl(sut.GoogleMobileLogin());

        Assert.NotEqual(state1, state2);
    }

    // ─── GoogleMobileCallBackAsync ────────────────────────────────────────────

    [Fact]
    public async Task GoogleMobileCallBackAsync_ReturnsUrlContainingJwtToken()
    {
        var sut     = CreateSut();
        var account = MakeAccount();
        var jwt     = MakeJwt("my-jwt");
        var (refresh, _) = MakeRefreshToken();

        var state = ExtractStateFromGoogleUrl(sut.GoogleMobileLogin("exp://192.168.1.1:8081/--/auth"));
        SetupFullGoogleFlow(account, jwt, refresh);

        var result = await sut.GoogleMobileCallBackAsync("auth-code", state, CancellationToken.None);

        Assert.Contains("token=my-jwt", result);
    }

    [Fact]
    public async Task GoogleMobileCallBackAsync_ReturnsUrlContainingHashedRefreshToken()
    {
        var sut     = CreateSut();
        var account = MakeAccount();
        var jwt     = MakeJwt();
        var (refresh, expectedHash) = MakeRefreshToken("known-raw-token");

        var state = ExtractStateFromGoogleUrl(sut.GoogleMobileLogin("exp://host/callback"));
        SetupFullGoogleFlow(account, jwt, refresh);

        var result = await sut.GoogleMobileCallBackAsync("code", state, CancellationToken.None);

        // The URL must contain the SHA-256 hash of the raw token, not the raw token itself
        Assert.Contains($"refreshToken={expectedHash}", result);
    }

    [Fact]
    public async Task GoogleMobileCallBackAsync_AppendsTokensWithQuestionMark_WhenNoExistingQuery()
    {
        var sut     = CreateSut();
        var account = MakeAccount();
        var (refresh, _) = MakeRefreshToken();

        const string returnUrl = "exp://192.168.1.1:8081/--/auth"; // no existing query
        var state = ExtractStateFromGoogleUrl(sut.GoogleMobileLogin(returnUrl));
        SetupFullGoogleFlow(account, MakeJwt(), refresh);

        var result = await sut.GoogleMobileCallBackAsync("code", state, CancellationToken.None);

        Assert.Contains("?token=", result);
    }

    [Fact]
    public async Task GoogleMobileCallBackAsync_AppendsTokensWithAmpersand_WhenQueryAlreadyExists()
    {
        var sut     = CreateSut();
        var account = MakeAccount();
        var (refresh, _) = MakeRefreshToken();

        const string returnUrl = "exp://192.168.1.1:8081/--/auth?source=social";
        var state = ExtractStateFromGoogleUrl(sut.GoogleMobileLogin(returnUrl));
        SetupFullGoogleFlow(account, MakeJwt(), refresh);

        var result = await sut.GoogleMobileCallBackAsync("code", state, CancellationToken.None);

        Assert.Contains("&token=", result);
    }

    [Fact]
    public async Task GoogleMobileCallBackAsync_RemovesStateFromCacheAfterUse()
    {
        var sut     = CreateSut();
        var account = MakeAccount();
        var (refresh, _) = MakeRefreshToken();
        SetupFullGoogleFlow(account, MakeJwt(), refresh);

        var state = ExtractStateFromGoogleUrl(sut.GoogleMobileLogin("exp://host/callback"));
        await sut.GoogleMobileCallBackAsync("code", state, CancellationToken.None);

        Assert.False(_memoryCache.TryGetValue(state, out _));
    }

    [Fact]
    public async Task GoogleMobileCallBackAsync_SavesRefreshTokenAndCommits()
    {
        var sut     = CreateSut();
        var account = MakeAccount();
        var (refresh, _) = MakeRefreshToken();
        SetupFullGoogleFlow(account, MakeJwt(), refresh);

        var state = ExtractStateFromGoogleUrl(sut.GoogleMobileLogin());
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
        var sut     = CreateSut();
        var account = MakeAccount();
        var (refresh, _) = MakeRefreshToken();
        SetupFullGoogleFlow(account, MakeJwt(), refresh);

        var state = ExtractStateFromGoogleUrl(sut.GoogleMobileLogin());
        await sut.GoogleMobileCallBackAsync("code", state, CancellationToken.None);

        // Second call with the same (now-consumed) state must fail
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.GoogleMobileCallBackAsync("code", state, CancellationToken.None));
    }

    // ─── LinkedInMobileLogin ──────────────────────────────────────────────────

    [Fact]
    public void LinkedInMobileLogin_ReturnsValidLinkedInOAuthUrl()
    {
        var sut = CreateSut();

        var url = sut.LinkedInMobileLogin();

        Assert.StartsWith("https://www.linkedin.com/oauth/v2/authorization", url);
    }

    [Fact]
    public void LinkedInMobileLogin_ContainsClientId()
    {
        var sut = CreateSut();

        var url = sut.LinkedInMobileLogin();

        Assert.Contains($"client_id={_settings.LinkedIn.ClientId}", url);
    }

    [Fact]
    public void LinkedInMobileLogin_ContainsEncodedRedirectUri()
    {
        var sut = CreateSut();
        var expected = Uri.EscapeDataString($"{_settings.BaseUrl}/social/auth/linkedin/mobile/callback");

        var url = sut.LinkedInMobileLogin();

        Assert.Contains($"redirect_uri={expected}", url);
    }

    [Fact]
    public void LinkedInMobileLogin_StoresReturnUrlInCache_WhenProvided()
    {
        var sut = CreateSut();
        const string returnUrl = "exp://192.168.1.1:8081/--/auth";

        var state = ExtractStateFromLinkedInUrl(sut.LinkedInMobileLogin(returnUrl));

        Assert.True(_memoryCache.TryGetValue(state, out string? cached));
        Assert.Equal(returnUrl, cached);
    }

    [Fact]
    public void LinkedInMobileLogin_StoresDefaultCallback_WhenReturnUrlIsNull()
    {
        var sut = CreateSut();

        var state = ExtractStateFromLinkedInUrl(sut.LinkedInMobileLogin(null));

        Assert.True(_memoryCache.TryGetValue(state, out string? cached));
        Assert.Equal("careerflow://auth/callback", cached);
    }

    [Fact]
    public void LinkedInMobileLogin_GeneratesUniqueStateOnEachCall()
    {
        var sut = CreateSut();

        var state1 = ExtractStateFromLinkedInUrl(sut.LinkedInMobileLogin());
        var state2 = ExtractStateFromLinkedInUrl(sut.LinkedInMobileLogin());

        Assert.NotEqual(state1, state2);
    }

    // ─── LinkedInCallBackAsync ────────────────────────────────────────────────

    [Fact]
    public async Task LinkedInCallBackAsync_ReturnsUrlContainingJwtToken()
    {
        var sut     = CreateSut();
        var account = MakeAccount();
        var jwt     = MakeJwt("linkedin-jwt");
        var (refresh, _) = MakeRefreshToken();

        var state = ExtractStateFromLinkedInUrl(sut.LinkedInMobileLogin("exp://host/callback"));
        SetupFullLinkedInFlow(account, jwt, refresh);

        var result = await sut.LinkedInCallBackAsync("li-code", state, CancellationToken.None);

        Assert.Contains("token=linkedin-jwt", result);
    }

    [Fact]
    public async Task LinkedInCallBackAsync_ReturnsUrlContainingHashedRefreshToken()
    {
        var sut     = CreateSut();
        var account = MakeAccount();
        var jwt     = MakeJwt();
        var (refresh, expectedHash) = MakeRefreshToken("known-linkedin-raw");

        var state = ExtractStateFromLinkedInUrl(sut.LinkedInMobileLogin("exp://host/callback"));
        SetupFullLinkedInFlow(account, jwt, refresh);

        var result = await sut.LinkedInCallBackAsync("code", state, CancellationToken.None);

        Assert.Contains($"refreshToken={expectedHash}", result);
    }

    [Fact]
    public async Task LinkedInCallBackAsync_RemovesStateFromCacheAfterUse()
    {
        var sut     = CreateSut();
        var account = MakeAccount();
        var (refresh, _) = MakeRefreshToken();
        SetupFullLinkedInFlow(account, MakeJwt(), refresh);

        var state = ExtractStateFromLinkedInUrl(sut.LinkedInMobileLogin());
        await sut.LinkedInCallBackAsync("code", state, CancellationToken.None);

        Assert.False(_memoryCache.TryGetValue(state, out _));
    }

    [Fact]
    public async Task LinkedInCallBackAsync_SavesRefreshTokenAndCommits()
    {
        var sut     = CreateSut();
        var account = MakeAccount();
        var (refresh, _) = MakeRefreshToken();
        SetupFullLinkedInFlow(account, MakeJwt(), refresh);

        var state = ExtractStateFromLinkedInUrl(sut.LinkedInMobileLogin());
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
        var sut     = CreateSut();
        var account = MakeAccount();
        var (refresh, _) = MakeRefreshToken();
        SetupFullLinkedInFlow(account, MakeJwt(), refresh);

        var state = ExtractStateFromLinkedInUrl(sut.LinkedInMobileLogin());
        await sut.LinkedInCallBackAsync("code", state, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.LinkedInCallBackAsync("code", state, CancellationToken.None));
    }

    // ─── Cross-provider isolation ─────────────────────────────────────────────

    [Fact]
    public async Task GoogleState_CannotBeReused_AfterGoogleCallbackConsumesIt()
    {
        var sut     = CreateSut();
        var account = MakeAccount();
        var (refresh, _) = MakeRefreshToken();
        SetupFullGoogleFlow(account, MakeJwt(), refresh);

        var state = ExtractStateFromGoogleUrl(sut.GoogleMobileLogin());
        await sut.GoogleMobileCallBackAsync("code", state, CancellationToken.None);

        // State has been consumed — LinkedIn callback must fail with the same token
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.LinkedInCallBackAsync("code", state, CancellationToken.None));
    }

    [Fact]
    public async Task TwoSimultaneousFlows_HaveIsolatedCacheEntries()
    {
        var sut = CreateSut();

        const string returnUrl1 = "exp://device1/callback";
        const string returnUrl2 = "exp://device2/callback";

        var googleState   = ExtractStateFromGoogleUrl(sut.GoogleMobileLogin(returnUrl1));
        var linkedInState = ExtractStateFromLinkedInUrl(sut.LinkedInMobileLogin(returnUrl2));

        Assert.NotEqual(googleState, linkedInState);
        Assert.True(_memoryCache.TryGetValue(googleState,   out string? g) && g == returnUrl1);
        Assert.True(_memoryCache.TryGetValue(linkedInState, out string? l) && l == returnUrl2);
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