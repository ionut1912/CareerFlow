using CareerFlow.Core.Application.CQRS.Legal.Command;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;
using System.Text.Json;

namespace CareerFlow.Core.Application.CQRS.Legal.Handler;

public class UpdateLegalDocCommandHandler
{
    private readonly ILegalDocRepository _legalDocRepository;
    private readonly ILogger<UpdateLegalDocCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public UpdateLegalDocCommandHandler(ILegalDocRepository legalDocRepository, ILogger<UpdateLegalDocCommandHandler> logger, IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _legalDocRepository = legalDocRepository ?? throw new ArgumentNullException(nameof(legalDocRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    public async Task<LegalDocDto> Handle(UpdateLegalDocCommand command, CancellationToken cancellationToken)
    {
        var legalDoc = await _legalDocRepository.GetLegalDocByTypeAsync(command.Type, cancellationToken);
        if (legalDoc is null)
        {
            _logger.LogError("No legal document found for type {Type}.", command.Type);
            throw new LegalDocNotFoundException($"No legal document found for type {command.Type}");
        }

        legalDoc.Update(command.Content, command.Type);
        _legalDocRepository.Update(legalDoc);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheService.SetCacheValueAsync($"LegalDoc_{command.Type}", legalDoc);
        var legalDocDto = legalDoc.ToDto();
        _logger.LogInformation("Legal document of type {Type} updated successfully,updatedDoc {legalDocDto}.", command.Type,
            JsonSerializer.Serialize(legalDoc, new JsonSerializerOptions { WriteIndented = true }));
        return legalDocDto;
    }
}
