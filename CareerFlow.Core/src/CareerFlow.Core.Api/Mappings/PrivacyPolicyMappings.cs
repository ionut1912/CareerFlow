using CareerFlow.Core.Api.Requests;
using CareerFlow.Core.Application.Mediatr.PrivacyPolicies.Commands;

namespace CareerFlow.Core.Api.Mappings;

public static class PrivacyPolicyMappings
{
    public static CreatePrivacyPolicyCommand ToCreatePrivacyPolicyCommand(this LegalRequest request)
        => new CreatePrivacyPolicyCommand(request.Content);

    public static UpdatePrivacyPolicyCommand ToUpdatePrivacyPolicyCommand(this LegalRequest request, Guid id)
        => new UpdatePrivacyPolicyCommand(id, request.Content);
}
