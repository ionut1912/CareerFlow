using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Models.Authentication;

namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface IJwtTokenService
{
    AuthResult GenerateToken(Account account);
    RefreshToken GenerateRefreshToken(Guid userId, string jwtId);
}