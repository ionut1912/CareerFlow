namespace CareerFlow.Core.Domain.Models.Authentication;

public record AuthResult(string Token, string Jti)
{
}
