using CareerFlow.Core.Api.Requests;
using CareerFlow.Core.Application.Mediatr.TermAndConditions.Commands;

namespace CareerFlow.Core.Api.Mappings
{
    public static class TermAndConditionMappings
    {
        public static CreateTermAndConditionsCommand ToCreateTermAndConditionsCommand(this LegalRequest request)
            => new CreateTermAndConditionsCommand(request.Content);
        public static UpdateTermsAndConditionsCommand ToUpdateTermsAndConditionsCommand(this LegalRequest request, Guid id)
            => new UpdateTermsAndConditionsCommand(id, request.Content);
    }
}
