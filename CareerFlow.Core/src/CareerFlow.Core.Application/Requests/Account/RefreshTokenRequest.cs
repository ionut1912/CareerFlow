namespace CareerFlow.Core.Application.Requests.Account;

public sealed record RefreshTokenRequest(string Token, string RefreshToken);