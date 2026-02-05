using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Domain.Entities;

namespace CareerFlow.Core.Application.Mappings;

public static class PrivacyPolicyMapping
{
    public static PrivacyPolicyDto ToPrivacyPolicyDto(this PrivacyPolicy privacyPolicy)
    {
        return new PrivacyPolicyDto(
            privacyPolicy.Id,
            privacyPolicy.Content,
            privacyPolicy.CreatedAt,
            privacyPolicy.UpdatedAt ?? new DateTime()
        );
    }
}
