namespace CareerFlow.Core.Application.Requests.Account;

public record CreateAccountRequest(string Email, string Password, string ConfirmPassword, string Username, string Name);
