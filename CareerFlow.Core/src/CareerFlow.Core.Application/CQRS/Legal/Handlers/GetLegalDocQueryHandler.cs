using CareerFlow.Core.Application.CQRS.Legal.Queries;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.Models.Legal;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Application.CQRS.Legal.Handlers;

public partial class GetLegalDocQueryHandler
{
    private static readonly HashSet<string> _validTypes =
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
        if (!_validTypes.Contains(request.Type))
        {
            LogInvalidType(request.Type);
            throw new LegalDocInvalidTypeException("Tipul precizat nu exista");
        }

        LegalDocumentResponse? document = await _legalService.GetDocumentAsync(request.Type, cancellationToken);

        if (document is not null) return document;
        LogDocumentNotFound(request.Type);
        throw new LegalDocNotFoundException("Documentul nu a fost gasit");
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Tipul precizat nu exista: {Type}")]
    private partial void LogInvalidType(string type);

    [LoggerMessage(Level = LogLevel.Error, Message = "Documentul nu a fost gasit pentru tipul {Type}")]
    private partial void LogDocumentNotFound(string type);
}
