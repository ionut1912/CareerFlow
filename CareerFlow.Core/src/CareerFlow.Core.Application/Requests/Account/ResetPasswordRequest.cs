namespace CareerFlow.Core.Application.Requests.Account;

public record ResetPasswordRequest(string Email, string NewPassword, string Token)
{
}