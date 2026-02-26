namespace CareerFlow.Core.Application.Requests;

public record CreateAccountRequest(string Email, string Password, string ConfirmPassword, string Username, string Name);