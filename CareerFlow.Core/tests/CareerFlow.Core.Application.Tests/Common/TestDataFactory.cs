using CareerFlow.Core.Domain.Entities;

namespace CareerFlow.Core.Application.Tests.Common;

public static class TestDataFactory
{
    public static Account CreateAccount()
        => Account.Create("test@email.com", "Password123!", "testUser", "Test Name");

    public static RefreshToken CreateRefreshToken(Guid userId)
        => RefreshToken.Create(userId, "refreshTokenVal", "jwtTokenVal", DateTime.UtcNow.AddDays(5));
}
