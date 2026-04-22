using System.Security.Cryptography;
using System.Text;
using CareerFlow.Core.Domain.Exceptions;

using JetBrains.Annotations;

using Shared.Domain.Common;

namespace CareerFlow.Core.Domain.Entities;

public class RefreshToken : Entity
{
    [UsedImplicitly]
    private RefreshToken()// For EfCore
    {
    }

    private RefreshToken(Guid userId, string rawToken, string jwtId, DateTime expiryDate)
    {
        if (userId == Guid.Empty)
            throw new InvalidFieldException("User id-ul este invalid");
        if (string.IsNullOrWhiteSpace(rawToken))
            throw new InvalidFieldException("Tokenul este invalid");
        if (string.IsNullOrWhiteSpace(jwtId))
            throw new InvalidFieldException("JWT id-ul este invalid");
        if (expiryDate == default)
            throw new InvalidFieldException("Data de expirare este invalida");
        if (expiryDate <= DateTime.UtcNow)
            throw new InvalidFieldException("Data de expirare este in trecut");

        UserId = userId;
        TokenHash = HashToken(rawToken);
        JwtId = jwtId;
        IsUsed = false;
        IsRevoked = false;
        CreatedAt = DateTime.UtcNow;
        ExpiryDate = expiryDate;
    }

    public Guid UserId { get; private set; }
    public Account Account { get; private set; } = null!;
    public string TokenHash { get; } = string.Empty;
    public string JwtId { get; private set; } = string.Empty;
    public bool IsUsed { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime ExpiryDate { get; private set; }

    public static RefreshToken Create(Guid userId, string rawToken, string jwtId, DateTime expiryDate) => new(userId, rawToken, jwtId, expiryDate);

    public bool VerifyToken(string rawToken) => TokenHash == HashToken(rawToken);

    public void MarkAsUsed()
    {
        IsUsed = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsRevoked()
    {
        IsRevoked = true;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string HashToken(string rawToken)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}
