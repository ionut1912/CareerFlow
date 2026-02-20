using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Shared.Infra.Services;

namespace CareerFlow.Core.Infrastructure.Persistance.Repositories;

public class LegalDocRepository(DbSet<LegalDoc> dbSet) : GenericRepository<LegalDoc>(dbSet), ILegalDocRepository
{
    public async Task<LegalDoc?> GetLegalDocByTypeAsync(string type, CancellationToken cancellationToken)
    {
        var docType = LegalDocType.FromString(type);
    
        return await dbSet.FirstOrDefaultAsync(l => l.Type == docType, cancellationToken);
    }
}