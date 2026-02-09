using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mediatr.TermAndConditions.Commands;
using CareerFlow.Core.Application.Requests;
using CareerFlow.Core.Domain.Entities;

namespace CareerFlow.Core.Application.Mappings;

public static class TermAndConditionsMappings
{
    public static TermsAndConditionDto ToTermAndConditionsDto(this TermsAndCondition termsAndCondition)
    {
        return new TermsAndConditionDto
        (
            termsAndCondition.Id,
            termsAndCondition.Content,
            termsAndCondition.CreatedAt,
           termsAndCondition.UpdatedAt ?? new DateTime()
        );
    }

    public static CreateTermAndConditionsCommand ToCreateTermAndConditionsCommand(this LegalRequest request)
    => new CreateTermAndConditionsCommand(request.Content);
    public static UpdateTermsAndConditionsCommand ToUpdateTermsAndConditionsCommand(this LegalRequest request)
        => new UpdateTermsAndConditionsCommand(request.Content);
}
