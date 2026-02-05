using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Domain.Entities;

namespace CareerFlow.Core.Application.Mappings;

public static class TermAndConditionsMappings
{
    public static TermsAndConditionDto ToDto(this TermsAndCondition termsAndCondition)
    {
        return new TermsAndConditionDto
        (
            termsAndCondition.Id,
            termsAndCondition.Content,
            termsAndCondition.Accepted,
            termsAndCondition.CreatedAt,
           termsAndCondition.UpdatedAt?? new DateTime()
        );
    }
}
