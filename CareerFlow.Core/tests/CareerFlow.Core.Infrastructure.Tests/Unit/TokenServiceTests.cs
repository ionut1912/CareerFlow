using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Unit;

public class TokenServiceTests
{
    private readonly TokenService _sut;
    private readonly string _validJwtToken;

    public TokenServiceTests()
    {
        var inMemory = new Dictionary<string, string?>
        {
            { "JwtSettings:Key", "SuperSecretKeyThatIsAtLeast32BytesLong!!" },
            { "JwtSettings:Issuer", "TestIssuer" },
            { "JwtSettings:Audience", "TestAudience" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemory)
            .Build();

        _sut = new TokenService(configuration);

        // Generate a real JWT once — reused across all refresh token tests
        _validJwtToken = _sut.GenerateToken(BuildTestAccount()).Token;
    }

    private static Account BuildTestAccount()
    {
        var account = Account.Create("user@test.com", "password", "testuser", "Test User");
        account.AcceptTerms();
        account.AcceptPrivacyPolicy();
        return account;
    }

    [Fact]
    public void GenerateToken_ValidAccount_ReturnsNonNullAuthResult()
    {
        var account = BuildTestAccount();
        var result = _sut.GenerateToken(account);
        result.ShouldNotBeNull();
        result.Token.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateToken_CalledTwice_ProducesDistinctJti()
    {
        var account = BuildTestAccount();
        var result1 = _sut.GenerateToken(account);
        var result2 = _sut.GenerateToken(account);
        result1.Jti.ShouldNotBe(result2.Jti);
    }

    [Fact]
    public void GenerateToken_ValidAccount_TokenContainsSubjectAndEmailClaims()
    {
        var account = BuildTestAccount();
        var result = _sut.GenerateToken(account);
        var handler = new JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(result.Token);
        jwt.Subject.ShouldBe(account.Id.ToString());
        jwt.GetClaim("email").Value.ShouldBe(account.Email);
    }

    [Fact]
    public void GenerateToken_AccountWithAcceptedTerms_TermsAndPolicyClaimsAreTrue()
    {
        var account = BuildTestAccount();
        var result = _sut.GenerateToken(account);
        var handler = new JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(result.Token);
        jwt.GetClaim("terms_accepted").Value.ShouldBe("true");
        jwt.GetClaim("policy_accepted").Value.ShouldBe("true");
    }

    [Fact]
    public void GenerateToken_NonFounderAccount_IsFounderClaimIsFalse()
    {
        var account = BuildTestAccount();
        var result = _sut.GenerateToken(account);
        var handler = new JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(result.Token);
        jwt.GetClaim("is_founder").Value.ShouldBe("false");
    }

    [Fact]
    public void GenerateToken_ValidAccount_JtiInTokenMatchesAuthResultJti()
    {
        var account = BuildTestAccount();
        var result = _sut.GenerateToken(account);
        var handler = new JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(result.Token);
        jwt.GetClaim(JwtRegisteredClaimNames.Jti).Value.ShouldBe(result.Jti);
    }

    [Fact]
    public void GenerateRefreshToken_ValidInputs_ReturnsRefreshTokenBoundToUser()
    {
        var userId = Guid.NewGuid();
        var refreshToken = _sut.GenerateRefreshToken(userId, _validJwtToken);
        refreshToken.ShouldNotBeNull();
        refreshToken.UserId.ShouldBe(userId);
    }

    [Fact]
    public void GenerateRefreshToken_ValidInputs_TokenStringIsNonEmpty()
    {
        var userId = Guid.NewGuid();
        var refreshToken = _sut.GenerateRefreshToken(userId, _validJwtToken);
        refreshToken.TokenHash.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateRefreshToken_CalledTwice_ProducesUniqueTokenStrings()
    {
        var userId = Guid.NewGuid();
        // Each call generates a new random token internally, so hashes will differ
        var rt1 = _sut.GenerateRefreshToken(userId, _validJwtToken);
        var rt2 = _sut.GenerateRefreshToken(userId, _validJwtToken);
        rt1.TokenHash.ShouldNotBe(rt2.TokenHash);
    }

    [Fact]
    public void GenerateRefreshToken_ValidInputs_ExpiresApproximatelySixMonthsFromNow()
    {
        var userId = Guid.NewGuid();
        var before = DateTime.UtcNow.AddMonths(6).AddSeconds(-5);
        var after = DateTime.UtcNow.AddMonths(6).AddSeconds(5);
        var refreshToken = _sut.GenerateRefreshToken(userId, _validJwtToken);
        refreshToken.ExpiryDate.ShouldBeInRange(before, after);
    }

    [Fact]
    public void GenerateRefreshToken_JwtId_MatchesJtiFromToken()
    {
        var account = BuildTestAccount();
        var authResult = _sut.GenerateToken(account);
        var refreshToken = _sut.GenerateRefreshToken(account.Id, authResult.Token);

        // JwtId stored on the refresh token must match the JTI embedded in the JWT
        refreshToken.JwtId.ShouldBe(authResult.Jti);
    }
}