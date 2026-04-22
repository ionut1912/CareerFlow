using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infra.Services;

namespace CareerFlow.Core.Infrastructure.Persistence.Repositories;

public class SystemDocumentRepository(DbSet<SystemDocument> systemDocuments)
    : GenericRepository<SystemDocument>(systemDocuments), ISystemDocumentRepository
{
    private readonly DbSet<SystemDocument> _systemDocuments = systemDocuments;

    public async Task<SystemDocument?> FindByTypeAsync(string type, CancellationToken cancellationToken) => await _systemDocuments.FirstOrDefaultAsync(d => d.DocumentType == type, cancellationToken);
}
