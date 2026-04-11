namespace CareerFlow.Core.Application.Requests.Account;

public record RefreshTokenRequest(string Token, string RefreshToken)
{
}