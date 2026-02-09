using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Application.Mediatr.PrivacyPolicies.Query;
using CareerFlow.Core.Application.Mediatr.TermAndConditions.Query;
using CareerFlow.Core.Application.Requests;
using Shared.Api.Endpoints;
using Shared.Api.Infrastructure;
using Shared.Application.Mediator;

namespace CareerFlow.Core.Api.Endpoints;

public class LegalEndpointGroup : EndpointGroup
{
    public override void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(this);
        group.MapPost(CreatePrivacyPolicy, "/privacy-policy");
        group.MapPut(UpdatePrivacyPolicy, "/privacy-policy");
        group.MapGet(GetPrivacyPolicy, "/privacy-policy");

        group.MapPost(CreateTermsAndConditions, "/terms-and-conditions");
        group.MapPut(UpdateTermsAndConditions, "/terms-and-conditions");
        group.MapGet(GetTermsAndConditions, "/terms-and-conditions");
    }

    private static async Task<IResult> CreatePrivacyPolicy(IMediator mediator, LegalRequest request, CancellationToken ct)
    {
        var createPrivacyPolicyCommand = request.ToCreatePrivacyPolicyCommand();
        var result = await mediator.Send(createPrivacyPolicyCommand, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> UpdatePrivacyPolicy(IMediator mediator, LegalRequest request, CancellationToken ct)
    {
        var updatePrivacyPolicyCommand = request.ToUpdatePrivacyPolicyCommand();
        var result = await mediator.Send(updatePrivacyPolicyCommand, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetPrivacyPolicy(IMediator mediator, CancellationToken ct)
    {
        var getPrivacyPolicyQuery = new GetPrivacyPolicyQuery();
        var result = await mediator.Send(getPrivacyPolicyQuery, ct);
        if (result == null)
        {
            return Results.NotFound("Politica nu a fost gasita.");
        }


        var htmlContent = $@"
    <!DOCTYPE html>
    <html lang='ro'>
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <title>Privacy Policy - CareerFlow</title>
        <style>
            body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; max-width: 800px; margin: 0 auto; padding: 20px; }}
            h1 {{ color: #0077b5; border-bottom: 2px solid #eee; padding-bottom: 10px; }}
            .container {{ background: #fff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
            .footer {{ margin-top: 40px; font-size: 0.8em; color: #777; text-align: center; }}
        </style>
    </head>
    <body>
        <div class='container'>
            <h1>Politica de Confidențialitate</h1>
            
            <div class='content'>
                {result.Content} 
            </div>

            <div class='footer'>
                &copy; {DateTime.Now.Year} CareerFlow. Toate drepturile rezervate.
            </div>
        </div>
    </body>
    </html>";

        // 2. Returnăm HTML cu Content-Type corect
        return Results.Content(htmlContent, "text/html");
    }

    private static async Task<IResult> CreateTermsAndConditions(IMediator mediator, LegalRequest request, CancellationToken ct)
    {
        var createTermsAndConditionsCommand = request.ToCreateTermAndConditionsCommand();
        var result = await mediator.Send(createTermsAndConditionsCommand, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateTermsAndConditions(IMediator mediator, LegalRequest request, CancellationToken ct)
    {
        var updateTermsAndConditionsCommand = request.ToUpdateTermsAndConditionsCommand();
        var result = await mediator.Send(updateTermsAndConditionsCommand, ct);
        return Results.Ok(result);
    }



    private static async Task<IResult> GetTermsAndConditions(IMediator mediator, CancellationToken ct)
    {
        var getTermsAndConditionsQuery = new GetTermAndConditionsQuery();
        var result = await mediator.Send(getTermsAndConditionsQuery, ct);
        if (result is null)
        {
            return Results.NotFound("Termenii și condițiile nu au fost găsite.");
        }

        var htmlContent = $@"
    <!DOCTYPE html>
    <html lang='ro'>
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <title>Termeni și Condiții - CareerFlow</title>
        <style>
            body {{ font-family: Helvetica, Arial, sans-serif; line-height: 1.6; color: #333; max-width: 800px; margin: 0 auto; padding: 40px 20px; }}
            h1 {{ color: #2c3e50; border-bottom: 2px solid #ecf0f1; padding-bottom: 15px; margin-bottom: 30px; }}
            .content {{ background: #f9f9f9; padding: 30px; border-radius: 8px; border: 1px solid #eee; white-space: pre-wrap; }}
            .footer {{ margin-top: 50px; font-size: 0.85em; color: #95a5a6; text-align: center; border-top: 1px solid #eee; padding-top: 20px; }}
        </style>
    </head>
    <body>
        <h1>Termeni și Condiții de Utilizare</h1>
        
        <div class='content'>
            {result.Content}
        </div>

        <div class='footer'>
            &copy; {DateTime.Now.Year} CareerFlow. Prin utilizarea aplicației, sunteți de acord cu acești termeni.
        </div>
    </body>
    </html>";
        return Results.Content(htmlContent, "text/html");
    }

}
