namespace CareerFlow.Core.Application.Dtos;

public record AccountDto(
    Guid Id,
    string Email,
    string Username,
    string? Token,
    bool IsFounder,
    bool PrivacyPolicyAccepted,
    bool TermsAccepted)
{
}