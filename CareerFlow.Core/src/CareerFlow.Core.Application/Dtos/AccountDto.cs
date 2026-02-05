namespace CareerFlow.Core.Application.Dtos;

public record AccountDto(string Email,
    string Username,
    string? Token,
    bool IsFounder)
{
}