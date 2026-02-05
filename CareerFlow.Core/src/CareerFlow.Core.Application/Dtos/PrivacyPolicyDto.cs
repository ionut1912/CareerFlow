namespace CareerFlow.Core.Application.Dtos;

public record PrivacyPolicyDto(Guid Id, string Content, bool Accepted, DateTime CreatedAt, DateTime UpdatedAt)
{
}
