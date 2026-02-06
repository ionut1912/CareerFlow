namespace CareerFlow.Core.Api.Requests;

public record RefreshTokenRequest(string Token, string RefreshToken)
{
}
