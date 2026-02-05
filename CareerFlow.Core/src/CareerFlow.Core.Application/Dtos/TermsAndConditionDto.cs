namespace CareerFlow.Core.Application.Dtos;

public record TermsAndConditionDto(Guid Id, string Content, bool Accepted, DateTime CreatedAt, DateTime UpdatedAt)
{
}
