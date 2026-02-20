using CareerFlow.Core.Application.CQRS.Legal.Command;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Application.CQRS.Legal.Handler;

public class CreateLegalDocCommandHandler
{
    private readonly ICacheService _cacheService;
    private readonly ILegalDocRepository _legalDocRepository;
    private readonly ILogger<CreateLegalDocCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLegalDocCommandHandler(ILegalDocRepository legalDocRepository,
        ILogger<CreateLegalDocCommandHandler> logger, IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        ArgumentNullException.ThrowIfNull(legalDocRepository);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(cacheService);
        _legalDocRepository = legalDocRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<Guid> Handle(CreateLegalDocCommand command, CancellationToken cancellationToken)
    {
        var legalDoc = LegalDoc.Create(command.Content, command.Type);
        await _legalDocRepository.AddAsync(legalDoc, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Documentul cu tipul {Type} a fost creat", legalDoc.Type);
        await _cacheService.SetCacheValueAsync($"LegalDoc_{legalDoc.Type.Value}", legalDoc.ToDto());
        return legalDoc.Id;
    }
}