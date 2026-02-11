using CareerFlow.Core.Application.CQRS.Legal.Command;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Requests;
using CareerFlow.Core.Domain.Entities;

namespace CareerFlow.Core.Application.Mappings;

public static class LegalDocMapping
{
    public static LegalDocDto ToDto(this LegalDoc legalDoc)
    {
        return new LegalDocDto(legalDoc.Content, legalDoc.Type.Value!);
    }

    public static CreateLegalDocCommand ToCreateLegalDocCommand(this LegalRequest legalRequest)
    {
        return new CreateLegalDocCommand(legalRequest.Content, legalRequest.Type);
    }

    public static UpdateLegalDocCommand ToUpdateLegalDocCommand(this LegalRequest legalRequest)
    {
        return new UpdateLegalDocCommand(legalRequest.Content, legalRequest.Type);
    }
}
