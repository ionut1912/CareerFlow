using CareerFlow.Core.Domain.Entities;

namespace CareerFlow.Core.Domain.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(Account account);
}