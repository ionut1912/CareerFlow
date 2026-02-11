using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Models.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CareerFlow.Core.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AuthResult GenerateToken(Account account)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jti = Guid.NewGuid().ToString();

 
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new(JwtRegisteredClaimNames.Name, account.Username), 
            new(JwtRegisteredClaimNames.Email, account.Email),
            new(JwtRegisteredClaimNames.Jti, jti),

     
            new("is_founder", account.IsFounder.ToString().ToLower()),
            new("terms_accepted", account.TermsAccepted.ToString().ToLower()),
            new("policy_accepted", account.PrivacyPolicyAccepted.ToString().ToLower())
        };


        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            Subject = new ClaimsIdentity(claims), 
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = creds
        };

  
        var handler = new JsonWebTokenHandler();
        var tokenString = handler.CreateToken(tokenDescriptor);

        return new AuthResult(tokenString, jti);
    }

    public RefreshToken GenerateRefreshToken(Guid userId, string jwtId)
    {
        var refreshToken = RefreshToken.Create(userId, jwtId, GenerateRandomString(35), DateTime.UtcNow.AddMonths(6));
        return refreshToken;
    }

    private static string GenerateRandomString(int length)
    {
        var random = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(random);
        return Convert.ToBase64String(random);
    }
}