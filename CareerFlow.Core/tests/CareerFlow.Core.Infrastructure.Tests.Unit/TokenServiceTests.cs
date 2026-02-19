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
        // Arrange
        var account = BuildTestAccount();

        // Act
        var result = _sut.GenerateToken(account);

        // Assert
        result.ShouldNotBeNull();
        result.Token.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateToken_CalledTwice_ProducesDistinctJti()
    {
        // Arrange
        var account = BuildTestAccount();

        // Act
        var result1 = _sut.GenerateToken(account);
        var result2 = _sut.GenerateToken(account);

        // Assert
        result1.Jti.ShouldNotBe(result2.Jti);
    }

    [Fact]
    public void GenerateToken_ValidAccount_TokenContainsSubjectAndEmailClaims()
    {
        // Arrange
        var account = BuildTestAccount();

        // Act
        var result = _sut.GenerateToken(account);
        var handler = new JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(result.Token);

        // Assert
        jwt.Subject.ShouldBe(account.Id.ToString());
        jwt.GetClaim("email").Value.ShouldBe(account.Email);
    }

    [Fact]
    public void GenerateToken_AccountWithAcceptedTerms_TermsAndPolicyClaimsAreTrue()
    {
        // Arrange
        var account = BuildTestAccount();

        // Act
        var result = _sut.GenerateToken(account);
        var handler = new JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(result.Token);

        // Assert
        jwt.GetClaim("terms_accepted").Value.ShouldBe("true");
        jwt.GetClaim("policy_accepted").Value.ShouldBe("true");
    }

    [Fact]
    public void GenerateToken_NonFounderAccount_IsFounderClaimIsFalse()
    {
        // Arrange
        var account = BuildTestAccount();

        // Act
        var result = _sut.GenerateToken(account);
        var handler = new JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(result.Token);

        // Assert
        jwt.GetClaim("is_founder").Value.ShouldBe("false");
    }

    [Fact]
    public void GenerateToken_ValidAccount_JtiInTokenMatchesAuthResultJti()
    {
        // Arrange
        var account = BuildTestAccount();

        // Act
        var result = _sut.GenerateToken(account);
        var handler = new JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(result.Token);

        // Assert
        jwt.GetClaim(JwtRegisteredClaimNames.Jti).Value.ShouldBe(result.Jti);
    }

    [Fact]
    public void GenerateRefreshToken_ValidInputs_ReturnsRefreshTokenBoundToUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string jwtToken = "some.jwt.token";

        // Act
        var refreshToken = _sut.GenerateRefreshToken(userId, jwtToken);

        // Assert
        refreshToken.ShouldNotBeNull();
        refreshToken.UserId.ShouldBe(userId);
    }

    [Fact]
    public void GenerateRefreshToken_ValidInputs_StoresJwtTokenReference()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string jwtToken = "original.jwt.token";

        // Act
        var refreshToken = _sut.GenerateRefreshToken(userId, jwtToken);

        // Assert
        refreshToken.JwtToken.ShouldBe(jwtToken);
    }

    [Fact]
    public void GenerateRefreshToken_ValidInputs_TokenStringIsNonEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var refreshToken = _sut.GenerateRefreshToken(userId, "jwt");

        // Assert
        refreshToken.Token.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateRefreshToken_CalledTwice_ProducesUniqueTokenStrings()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var rt1 = _sut.GenerateRefreshToken(userId, "jwt");
        var rt2 = _sut.GenerateRefreshToken(userId, "jwt");

        // Assert
        rt1.Token.ShouldNotBe(rt2.Token);
    }

    [Fact]
    public void GenerateRefreshToken_ValidInputs_ExpiresApproximatelySixMonthsFromNow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var before = DateTime.UtcNow.AddMonths(6).AddSeconds(-5);
        var after = DateTime.UtcNow.AddMonths(6).AddSeconds(5);

        // Act
        var refreshToken = _sut.GenerateRefreshToken(userId, "jwt");

        // Assert
        refreshToken.ExpiryDate.ShouldBeInRange(before, after);
    }
}