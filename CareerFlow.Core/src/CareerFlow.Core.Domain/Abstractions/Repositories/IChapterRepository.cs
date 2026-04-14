using CareerFlow.Core.Domain.Entities;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Domain.Abstractions.Repositories;

public interface IChapterRepository : IGenericRepository<Chapter>
{
    Task<bool> ExistsAsync(Guid chapterId, Guid courseId, CancellationToken ct);
}