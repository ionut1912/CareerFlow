namespace CareerFlow.Core.Application.Requests;

public record ResetPasswordRequest(string Email, string NewPassword, string Token)
{
}