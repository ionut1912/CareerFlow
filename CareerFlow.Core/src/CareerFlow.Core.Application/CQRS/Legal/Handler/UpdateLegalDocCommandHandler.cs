using CareerFlow.Core.Application.CQRS.Legal.Command;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Rabbit.Events.Events;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;
using System.Text.Json;
using Wolverine;

namespace CareerFlow.Core.Application.CQRS.Legal.Handler;

public class UpdateLegalDocCommandHandler
{
    private readonly ILegalDocRepository _legalDocRepository;
    private readonly ILogger<UpdateLegalDocCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public UpdateLegalDocCommandHandler(ILegalDocRepository legalDocRepository, ILogger<UpdateLegalDocCommandHandler> logger, IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        ArgumentNullException.ThrowIfNull(legalDocRepository, nameof(legalDocRepository));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(unitOfWork, nameof(unitOfWork));
        ArgumentNullException.ThrowIfNull(cacheService, nameof(cacheService));
        _legalDocRepository = legalDocRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<(LegalDocDto, OutgoingMessages)> Handle(UpdateLegalDocCommand command, CancellationToken cancellationToken)
    {
        var legalDoc = await _legalDocRepository.GetLegalDocByTypeAsync(command.Type, cancellationToken);
        if (legalDoc is null)
        {
            _logger.LogError("Nu a fost gasit documentul cu tipul {Type}.", command.Type);
            throw new LegalDocNotFoundException($"Nu a fost gasit documentul cu tipul {command.Type}");
        }

        legalDoc.Update(command.Content, command.Type);
        _legalDocRepository.Update(legalDoc);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var legalDocDto = legalDoc.ToDto();
        await _cacheService.SetCacheValueAsync($"LegalDoc_{command.Type}", legalDocDto);
        _logger.LogInformation("Documentul cu tipul {Type} modificat cu succes, documentul updatat {legalDocDto}.", command.Type,
            JsonSerializer.Serialize(legalDoc, new JsonSerializerOptions { WriteIndented = true }));

        var messages = new OutgoingMessages
        {
            new UpdateLegalDocsMessage()
        };

        return (legalDocDto, messages);
    }
}
