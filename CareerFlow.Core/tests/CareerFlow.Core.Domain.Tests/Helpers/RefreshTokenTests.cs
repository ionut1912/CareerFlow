using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using Shouldly;

namespace CareerFlow.Core.Domain.Tests.Helpers;

public class RefreshTokenTests
{
    [Fact]
    public void RefreshTokenCreate_ValidField_CreatesToken()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var token = "testToken";
        var jwtToken = "testJwt";
        var expiryDate = DateTime.UtcNow.AddDays(2);

        //Act
        var refreshToken = RefreshToken.Create(userId, token, jwtToken, expiryDate);

        //Assert
        refreshToken.UserId.ShouldBe(userId);
        refreshToken.Token.ShouldBe(token);
        refreshToken.JwtToken.ShouldBe(jwtToken);
        refreshToken.IsUsed.ShouldBeFalse();
        refreshToken.IsRevoked.ShouldBeFalse();
        refreshToken.ExpiryDate.ShouldBe(expiryDate);
    }

    [Theory]
    [MemberData(nameof(GetInvalidGuids))]
    public void RefreshTokenCreate_InvalidUserId_ThrowsException(Guid userId)
    {
        //Arrannge
        var token = "testToken";
        var jwtToken = "testJwt";
        var expiryDate = DateTime.UtcNow.AddDays(2);

        //Act
        var exception =
            Should.Throw<InvalidFieldException>(() => RefreshToken.Create(userId, token, jwtToken, expiryDate));

        //Assert
        exception.Message.ShouldBe("User id-ul este invalid");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void RefreshTokenCreate_InvalidToken_ThrowsException(string token)
    {
        //Arrannge
        var userId = Guid.NewGuid();
        var jwtToken = "testJwt";
        var expiryDate = DateTime.UtcNow.AddDays(2);

        //Act
        var exception = Should.Throw<InvalidFieldException>(() =>
            RefreshToken.Create(userId, token, jwtToken, expiryDate));

        //Assert
        exception.Message.ShouldBe("Tokenul este invalid");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void RefreshTokenCreate_InvalidJwtToken_ThrowsException(string jwtToken)
    {
        //Arrange
        var userId = Guid.NewGuid();
        var token = "testToken";
        var expiryDate = DateTime.UtcNow.AddDays(2);

        //Act
        var exception = Should.Throw<InvalidFieldException>(() =>
            RefreshToken.Create(userId, token, jwtToken, expiryDate));

        //Assert
        exception.Message.ShouldBe("Jwt-ul este invalid");
    }

    [Fact]
    public void RefreshTokenCreate_DefaultDate_ThrowsException()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var token = "testToken";
        var jwtToken = "testJwt";

        //Act
        var exception = Should.Throw<InvalidFieldException>(() =>
            RefreshToken.Create(userId, token, jwtToken, default));

        //Assert
        exception.Message.ShouldBe("Data de expirare este invalida");
    }

    [Fact]
    public void RefreshTokenCreate_DateInThePast_ThrowsException()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var token = "testToken";
        var jwtToken = "testJwt";
        var expiryDate = DateTime.UtcNow.AddDays(-2);


        //Act
        var exception = Should.Throw<InvalidFieldException>(() =>
            RefreshToken.Create(userId, token, jwtToken, expiryDate));

        //Assert
        exception.Message.ShouldBe("Data de expirare este in trecut");
    }

    [Fact]
    public void RefreshToken_MarkAsUsed_ModifyUsed()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var token = "testToken";
        var jwtToken = "testJwt";
        var expiryDate = DateTime.UtcNow.AddDays(2);
        var refreshToken = RefreshToken.Create(userId, token, jwtToken, expiryDate);

        //Act
        refreshToken.MarkAsUsed();

        //Assert
        refreshToken.UserId.ShouldBe(userId);
        refreshToken.Token.ShouldBe(token);
        refreshToken.JwtToken.ShouldBe(jwtToken);
        refreshToken.IsUsed.ShouldBeTrue();
        refreshToken.IsRevoked.ShouldBeFalse();
        refreshToken.ExpiryDate.ShouldBe(expiryDate);
    }

    [Fact]
    public void RefreshToken_MarkAsRevoked_ModifyRevoked()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var token = "testToken";
        var jwtToken = "testJwt";
        var expiryDate = DateTime.UtcNow.AddDays(2);
        var refreshToken = RefreshToken.Create(userId, token, jwtToken, expiryDate);

        //Act
        refreshToken.MarkAsRevoked();

        //Assert
        refreshToken.UserId.ShouldBe(userId);
        refreshToken.Token.ShouldBe(token);
        refreshToken.JwtToken.ShouldBe(jwtToken);
        refreshToken.IsUsed.ShouldBeFalse();
        refreshToken.IsRevoked.ShouldBeTrue();
        refreshToken.ExpiryDate.ShouldBe(expiryDate);
    }

    public static IEnumerable<object[]> GetInvalidGuids()
    {
        yield return new object[] { Guid.Empty };
    }
}