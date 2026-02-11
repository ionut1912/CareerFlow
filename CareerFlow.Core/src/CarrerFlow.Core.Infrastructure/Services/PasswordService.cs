using CareerFlow.Core.Domain.Abstractions.Services;

namespace CareerFlow.Core.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    // Work factor (cost) default 10-12 recommended; 12 is a reasonable secure default
    private const int WorkFactor = 12;

    public string HashPassword(string password)
    {
        // automatically generates salt + hash
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string hashed)
    {
        // Returns true if matches
        return BCrypt.Net.BCrypt.Verify(password, hashed);
    }
}