using CareerFlow.Core.Api.Mappings;
using CareerFlow.Core.Api.Requests;
using CareerFlow.Core.Application.Mediatr.PrivacyPolicies.Commands;
using CareerFlow.Core.Application.Mediatr.PrivacyPolicies.Query;
using CareerFlow.Core.Application.Mediatr.TermAndConditions.Commands;
using CareerFlow.Core.Application.Mediatr.TermAndConditions.Query;
using Shared.Api.Endpoints;
using Shared.Api.Infrastructure;
using Shared.Application.Mediator;

namespace CareerFlow.Core.Api.Endpoints;

public class LegalEndpointGroup:EndpointGroup
{
    public override void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(this);
        group.MapPost(CreatePrivacyPolicy, "/privacy-policy");
        group.MapPut(UpdatePrivacyPolicy, "/privacy-policy/{id:guid}");
        group.MapPut(AcceptPrivacyPolicy, "/privacy-policy/{id:guid}/accept");
        group.MapGet(GetPrivacyPolicy, "/privacy-policy/{id:guid}");

        group.MapPost(CreateTermsAndConditions, "/terms-and-conditions");
        group.MapPut(UpdateTermsAndConditions, "/terms-and-conditions/{id:guid}");
        group.MapPut(AcceptTermsAndConditions, "/terms-and-conditions/{id:guid}/accept");
        group.MapGet(GetTermsAndConditions, "/terms-and-conditions/{id:guid}");
    }

    private static async Task<IResult> CreatePrivacyPolicy(IMediator mediator, LegalRequest request, CancellationToken ct)
    {
        var createPrivacyPolicyCommand = request.ToCreatePrivacyPolicyCommand();
        var result = await mediator.Send(createPrivacyPolicyCommand, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> UpdatePrivacyPolicy(IMediator mediator, Guid id, LegalRequest request, CancellationToken ct)
    {
        var updatePrivacyPolicyCommand = request.ToUpdatePrivacyPolicyCommand(id);
        var result = await mediator.Send(updatePrivacyPolicyCommand, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> AcceptPrivacyPolicy(IMediator mediator, Guid id, CancellationToken ct)
    {
        var acceptPrivacyPolicyCommand = new AcceptPrivacyPolicyCommand(id);
        await mediator.Send(acceptPrivacyPolicyCommand, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> GetPrivacyPolicy(IMediator mediator, Guid id, CancellationToken ct)
    {
        var getPrivacyPolicyQuery = new GetPrivacyPolicyQuery(id);
        var result = await mediator.Send(getPrivacyPolicyQuery, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateTermsAndConditions(IMediator mediator, LegalRequest request, CancellationToken ct)
    {
        var createTermsAndConditionsCommand = request.ToCreateTermAndConditionsCommand();
        var result = await mediator.Send(createTermsAndConditionsCommand, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateTermsAndConditions(IMediator mediator, Guid id, LegalRequest request, CancellationToken ct)
    {
        var updateTermsAndConditionsCommand = request.ToUpdateTermsAndConditionsCommand(id);
        var result = await mediator.Send(updateTermsAndConditionsCommand, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> AcceptTermsAndConditions(IMediator mediator, Guid id, CancellationToken ct)
    {
        var acceptTermsAndConditionsCommand = new AcceptTermAndConditionsCommand(id);
        await mediator.Send(acceptTermsAndConditionsCommand, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> GetTermsAndConditions(IMediator mediator, Guid id, CancellationToken ct)
    {
        var getTermsAndConditionsQuery = new GetTermAndConditionsQuery(id);
        var result = await mediator.Send(getTermsAndConditionsQuery, ct);
        return Results.Ok(result);
    }
}
