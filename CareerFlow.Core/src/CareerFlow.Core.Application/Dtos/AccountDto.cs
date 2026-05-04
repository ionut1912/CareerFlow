namespace CareerFlow.Core.Application.Dtos;

public sealed record AccountDto(
    Guid Id,
    string Email,
    string Username,
    string Name,
    string? Token,
    string? RefreshToken,
    bool IsFounder,
    bool PrivacyPolicyAccepted,
    bool TermsAccepted);
