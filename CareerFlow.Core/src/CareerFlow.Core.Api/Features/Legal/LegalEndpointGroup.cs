using CareerFlow.Core.Application.CQRS.Legal.Queries;
using CareerFlow.Core.Domain.Models.Legal;
using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Api.Endpoints;
using Shared.Api.Infrastructure;
using Wolverine;

namespace CareerFlow.Core.Api.Features.Legal;

public class LegalEndpointGroup : EndpointGroup
{
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(this);
        group.MapGet(GetLegalDoc);
    }

    private async Task<Ok<LegalDocumentResponse>> GetLegalDoc(IMessageBus bus, string type, CancellationToken cancellationToken)
    {
        var getLegalDocQuery = new GetLegalDocQuery(type);
        var response = await bus.InvokeAsync<LegalDocumentResponse>(getLegalDocQuery, cancellationToken);
        return TypedResults.Ok(response);
    }
}