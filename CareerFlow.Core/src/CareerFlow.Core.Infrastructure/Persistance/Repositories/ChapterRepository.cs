using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infra.Services;

namespace CareerFlow.Core.Infrastructure.Persistance.Repositories;

public class ChapterRepository(DbSet<Chapter> dbSet) : GenericRepository<Chapter>(dbSet), IChapterRepository
{
    public async Task<bool> ExistsAsync(Guid chapterId, Guid courseId, CancellationToken ct)
    {
        return await dbSet
            .AnyAsync(c => c.Id == chapterId && c.CourseId == courseId, ct);
    }
}