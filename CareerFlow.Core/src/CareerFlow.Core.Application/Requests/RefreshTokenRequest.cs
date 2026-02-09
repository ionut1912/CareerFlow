namespace CareerFlow.Core.Application.Requests;

public record RefreshTokenRequest(string Token, string RefreshToken)
{
}
