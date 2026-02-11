using CareerFlow.Core.Application.CQRS.Legal.Query;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Application.Requests;
using Shared.Api.Endpoints;
using Shared.Api.Infrastructure;
using Wolverine;

namespace CareerFlow.Core.Api.Endpoints;

public class LegalEndpointGroup : EndpointGroup
{
    public override void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(this);
        group.MapPost(CreateLegalDoc);
        group.MapPut(UpdateLegalDoc);
        group.MapGet(GetLegalDoc);
    }

    private static async Task<IResult> CreateLegalDoc(IMessageBus bus, LegalRequest request, CancellationToken ct)
    {
        var createLegalDocCommand = request.ToCreateLegalDocCommand();
        var result = await bus.InvokeAsync<Guid>(createLegalDocCommand, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateLegalDoc(IMessageBus bus, LegalRequest request, CancellationToken ct)
    {
        var updateLegalDocCommand = request.ToUpdateLegalDocCommand();
        var result = await bus.InvokeAsync<LegalDocDto>(updateLegalDocCommand, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetLegalDoc(IMessageBus bus, string type, CancellationToken ct)
    {
        var getLegalDocQuery = new GetLegalDocQuery(type);
        var result = await bus.InvokeAsync<LegalDocDto>(getLegalDocQuery, ct);
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

}
