using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Domain.Test;

public class RefreshTokenEntityTests
{
    private static readonly DateTime Future = DateTime.UtcNow.AddDays(7);
 
    [Fact]
    public void Create_ValidParameters_ReturnsRefreshToken()
    {
        var userId = Guid.NewGuid();
 
        var token = RefreshToken.Create(userId, "raw-token", "jwt-id", Future);
 
        token.ShouldNotBeNull();
        token.UserId.ShouldBe(userId);
        token.JwtId.ShouldBe("jwt-id");
        token.IsUsed.ShouldBeFalse();
        token.IsRevoked.ShouldBeFalse();
    }
 
    [Fact]
    public void Create_EmptyUserId_ThrowsInvalidFieldException()
    {
        Should.Throw<InvalidFieldException>(() =>
            RefreshToken.Create(Guid.Empty, "raw", "jwt", Future));
    }
 
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_InvalidRawToken_ThrowsInvalidFieldException(string? raw)
    {
        Should.Throw<InvalidFieldException>(() =>
            RefreshToken.Create(Guid.NewGuid(), raw!, "jwt", Future));
    }
 
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_InvalidJwtId_ThrowsInvalidFieldException(string? jwtId)
    {
        Should.Throw<InvalidFieldException>(() =>
            RefreshToken.Create(Guid.NewGuid(), "raw", jwtId!, Future));
    }
 
    [Fact]
    public void Create_DefaultExpiryDate_ThrowsInvalidFieldException()
    {
        Should.Throw<InvalidFieldException>(() =>
            RefreshToken.Create(Guid.NewGuid(), "raw", "jwt", default));
    }
 
    [Fact]
    public void Create_PastExpiryDate_ThrowsInvalidFieldException()
    {
        Should.Throw<InvalidFieldException>(() =>
            RefreshToken.Create(Guid.NewGuid(), "raw", "jwt", DateTime.UtcNow.AddDays(-1)));
    }
 
    [Fact]
    public void Create_HashesRawToken_HashDiffersFromRaw()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "raw-token", "jwt", Future);
 
        token.TokenHash.ShouldNotBe("raw-token");
        token.TokenHash.ShouldNotBeEmpty();
    }
 
    [Fact]
    public void Create_SameRawToken_ProducesSameHash()
    {
        var t1 = RefreshToken.Create(Guid.NewGuid(), "raw-token", "jwt1", Future);
        var t2 = RefreshToken.Create(Guid.NewGuid(), "raw-token", "jwt2", Future);
 
        t1.TokenHash.ShouldBe(t2.TokenHash);
    }
 
    [Fact]
    public void VerifyToken_CorrectRawToken_ReturnsTrue()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "raw-token", "jwt", Future);
 
        token.VerifyToken("raw-token").ShouldBeTrue();
    }
 
    [Fact]
    public void VerifyToken_IncorrectRawToken_ReturnsFalse()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "raw-token", "jwt", Future);
 
        token.VerifyToken("wrong").ShouldBeFalse();
    }
 
    [Fact]
    public void MarkAsUsed_SetsIsUsedTrue()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "raw", "jwt", Future);
 
        token.MarkAsUsed();
 
        token.IsUsed.ShouldBeTrue();
    }
 
    [Fact]
    public void MarkAsRevoked_SetsIsRevokedTrue()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "raw", "jwt", Future);
 
        token.MarkAsRevoked();
 
        token.IsRevoked.ShouldBeTrue();
    }
}
