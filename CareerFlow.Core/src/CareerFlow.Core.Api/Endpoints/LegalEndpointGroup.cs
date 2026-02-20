using System.Net;
using CareerFlow.Core.Application.CQRS.Legal.Query;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Application.Requests;
using Markdig;
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

    private static async Task<IResult> CreateLegalDoc(IMessageBus bus, LegalRequest request,
        CancellationToken cancellationToken)
    {
        var createLegalDocCommand = request.ToCreateLegalDocCommand();
        var result = await bus.InvokeAsync<Guid>(createLegalDocCommand, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateLegalDoc(IMessageBus bus, LegalRequest request,
        CancellationToken cancellationToken)
    {
        var updateLegalDocCommand = request.ToUpdateLegalDocCommand();
        var result = await bus.InvokeAsync<LegalDocDto>(updateLegalDocCommand, cancellationToken);
        return Results.Ok(result);
    }

private static async Task<IResult> GetLegalDoc(IMessageBus bus, string type, CancellationToken cancellationToken)
{
    var getLegalDocQuery = new GetLegalDocQuery(type);
    var result = await bus.InvokeAsync<LegalDocDto>(getLegalDocQuery, cancellationToken);

    var pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    var htmlBody = Markdown.ToHtml(result.Content, pipeline);

    var htmlContent = $@"
<!DOCTYPE html>
<html lang='ro'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Privacy Policy - CareerFlow</title>
    <style>
        body {{ 
            font-family: 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; 
            line-height: 1.7; 
            color: #2c3e50; 
            max-width: 850px; 
            margin: 0 auto; 
            padding: 40px 20px; 
            background-color: #f4f7f9; 
        }}
        .container {{ 
            background: #ffffff; 
            padding: 40px; 
            border-radius: 12px; 
            box-shadow: 0 10px 25px rgba(0,0,0,0.05); 
        }}
        h1, h2, h3 {{ color: #1a73e8; font-weight: 600; }}
        h1 {{ font-size: 2.2em; margin-bottom: 0.8em; border-bottom: 2px solid #eef2f7; padding-bottom: 15px; }}
        h2 {{ font-size: 1.6em; margin-top: 1.5em; }}
        h3 {{ font-size: 1.2em; margin-top: 1.2em; }}
        p {{ margin-bottom: 1.2em; }}
        ul {{ padding-left: 25px; margin-bottom: 1.2em; }}
        li {{ margin-bottom: 0.5em; }}
        strong {{ color: #1a73e8; }}
        .footer {{ 
            margin-top: 50px; 
            font-size: 0.9em; 
            color: #95a5a6; 
            text-align: center; 
            border-top: 1px solid #eee; 
            padding-top: 20px; 
        }}
    </style>
</head>
<body>
    <div class='container'>
        {htmlBody}
        <div class='footer'>
            &copy; {DateTime.Now.Year} CareerFlow. Toate drepturile rezervate.
        </div>
    </div>
</body>
</html>";

    return Results.Content(htmlContent, "text/html");
}
}