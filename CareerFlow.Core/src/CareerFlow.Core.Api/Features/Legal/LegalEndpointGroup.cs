using CareerFlow.Core.Application.CQRS.Legal.Queries;
using CareerFlow.Core.Domain.Models.Legal;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Api.Endpoints;
using Shared.Api.Infrastructure;
using Wolverine;

namespace CareerFlow.Core.Api.Features.Legal;

[UsedImplicitly]
public class LegalEndpointGroup : EndpointGroup
{
    public override void Map(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup(this);
        group.MapGet(GetLegalDoc);
    }

    private static async Task<Ok<LegalDocumentResponse>> GetLegalDoc(IMessageBus bus, string type,
        CancellationToken cancellationToken)
    {
        var getLegalDocQuery = new GetLegalDocQuery(type);
        LegalDocumentResponse response = await bus.InvokeAsync<LegalDocumentResponse>(getLegalDocQuery, cancellationToken);
        return TypedResults.Ok(response);
    }
}
