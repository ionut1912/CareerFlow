using CareerFlow.Core.Application.CQRS.Legal.Queries;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.Models.Legal;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Application.CQRS.Legal.Handlers;

public class GetLegalDocQueryHandler
{
    private static readonly HashSet<string> ValidTypes =
        new(StringComparer.OrdinalIgnoreCase) { "privacy", "terms" };

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
        if (!ValidTypes.Contains(request.Type))
        {
            _logger.LogError("Tipul precizat nu exista: {Type}", request.Type);
            throw new LegalDocInvalidTypeException("Tipul precizat nu exista");
        }

        var document = await _legalService.GetDocumentAsync(request.Type, cancellationToken);

        if (document is null)
        {
            _logger.LogError("Documentul nu a fost gasit pentru tipul {Type}", request.Type);
            throw new LegalDocNotFoundException("Documentul nu a fost gasit");
        }

        return document;
    }
}