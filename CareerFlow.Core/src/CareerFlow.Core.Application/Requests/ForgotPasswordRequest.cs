namespace CareerFlow.Core.Application.Requests;

public record ForgotPasswordRequest(string Email, string ResetPasswordLink);