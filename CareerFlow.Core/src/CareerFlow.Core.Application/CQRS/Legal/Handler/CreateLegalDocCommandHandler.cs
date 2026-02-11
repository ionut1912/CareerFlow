using CareerFlow.Core.Application.CQRS.Legal.Command;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Application.CQRS.Legal.Handlers;

public class CreateLegalDocCommandHandler
{
    private readonly ILegalDocRepository _legalDocRepository;
    private readonly ILogger<CreateLegalDocCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public CreateLegalDocCommandHandler(ILegalDocRepository legalDocRepository, ILogger<CreateLegalDocCommandHandler> logger, IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _legalDocRepository = legalDocRepository ?? throw new ArgumentNullException(nameof(legalDocRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
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
