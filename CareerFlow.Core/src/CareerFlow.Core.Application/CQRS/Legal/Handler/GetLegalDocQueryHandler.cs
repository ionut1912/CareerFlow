using CareerFlow.Core.Application.CQRS.Legal.Query;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.Models;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Application.CQRS.Legal.Handler;

public class GetLegalDocQueryHandler
{
    private readonly ILegalService _legalService;
    private readonly ILogger<GetLegalDocQueryHandler> _logger;

    public GetLegalDocQueryHandler(ILegalService legalService, ILogger<GetLegalDocQueryHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(legalService);
        ArgumentNullException.ThrowIfNull(logger);
        _legalService = legalService;
        _logger = logger;
    }

    public async Task<LegalDocumentResponse> Handle(GetLegalDocQuery request, CancellationToken cancellationToken)
    {
        if (request.Type.ToLower() != "privacy" && request.Type.ToLower() != "terms")
        {
            _logger.LogError("Tipul precizat nu exista {type}", request.Type.ToLower());
            throw new LegalDocInvalidType("Tipul precizat nu exista");
        }

        var document = await _legalService.GetDocumentAsync(request.Type, cancellationToken);

        if (document == null)
        {
            _logger.LogError("Documentul nu a fost gasit");
            throw new LegalDocNotFound("Documentul nu a fost gasit");
        }

        return document;
    }
}