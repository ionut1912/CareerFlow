using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Models.Authentication;

namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface ITokenService
{
    AuthResult GenerateToken(Account account);
    RefreshToken GenerateRefreshToken(Guid userId, string jwtToken);
}