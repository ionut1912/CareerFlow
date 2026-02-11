using CareerFlow.Core.Domain.Entities;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Domain.Abstractions.Repositories;

public interface ILegalDocRepository : IGenericRepository<LegalDoc>
{
    Task<LegalDoc?> GetLegalDocByTypeAsync(string type, CancellationToken cancellationToken);
}
