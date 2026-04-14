namespace CareerFlow.Core.Application.Requests.Account;


public sealed record ResetPasswordRequest(string Email, string NewPassword, string Token);

