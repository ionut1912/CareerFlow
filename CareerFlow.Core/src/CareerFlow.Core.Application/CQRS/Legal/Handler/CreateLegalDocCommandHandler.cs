using CareerFlow.Core.Application.CQRS.Legal.Command;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Application.CQRS.Legal.Handler;

public class CreateLegalDocCommandHandler
{
    private readonly ILegalDocRepository _legalDocRepository;
    private readonly ILogger<CreateLegalDocCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public CreateLegalDocCommandHandler(ILegalDocRepository legalDocRepository, ILogger<CreateLegalDocCommandHandler> logger, IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        ArgumentNullException.ThrowIfNull(legalDocRepository, nameof(legalDocRepository));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(unitOfWork, nameof(unitOfWork));
        ArgumentNullException.ThrowIfNull(cacheService, nameof(cacheService));
        _legalDocRepository = legalDocRepository ?? throw new ArgumentNullException(nameof(legalDocRepository));
        _logger = logger;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<Guid> Handle(CreateLegalDocCommand command, CancellationToken ct)
    {
        var legalDoc = LegalDoc.Create(command.Content, command.Type);
        await _legalDocRepository.AddAsync(legalDoc, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Legal document created with ID: {LegalDocId}", legalDoc.Id);
        await _cacheService.SetCacheValueAsync($"LegalDoc_{LegalDocType.FromString(command.Type).Value}", legalDoc.ToDto());
        return legalDoc.Id;
    }
}
