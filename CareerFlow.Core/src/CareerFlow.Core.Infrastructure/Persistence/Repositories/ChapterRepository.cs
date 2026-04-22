using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infra.Services;

namespace CareerFlow.Core.Infrastructure.Persistence.Repositories;

public class ChapterRepository(DbSet<Chapter> chapters) : GenericRepository<Chapter>(chapters), IChapterRepository
{
    private readonly DbSet<Chapter> _chapters = chapters;

    public async Task<bool> ExistsAsync(Guid chapterId, Guid courseId, CancellationToken ct)
    {
        return await _chapters
            .AnyAsync(c => c.Id == chapterId && c.CourseId == courseId, ct);
    }
}