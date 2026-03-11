using CareerFlow.Core.Application.CQRS.Legal.Query;
using CareerFlow.Core.Domain.Models;
using CareerFlow.Core.Domain.Models.Legal;
using Shared.Api.Endpoints;
using Shared.Api.Infrastructure;
using Wolverine;

namespace CareerFlow.Core.Api.Endpoints;

public class LegalEndpointGroup : EndpointGroup
{
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(this);
        group.MapGet(GetLegalDoc);
    }

    private async Task<IResult> GetLegalDoc(IMessageBus bus, string type, CancellationToken cancellationToken)
    {
        var getLegalDocQuery = new GetLegalDocQuery(type);
        var response = await bus.InvokeAsync<LegalDocumentResponse>(getLegalDocQuery, cancellationToken);
        return Results.Ok(response);
    }
}