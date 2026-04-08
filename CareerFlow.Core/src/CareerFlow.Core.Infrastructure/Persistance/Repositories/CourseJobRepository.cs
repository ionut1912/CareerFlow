using CareerFlow.Core.Application.Responses;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infra.Services;

namespace CareerFlow.Core.Infrastructure.Persistance.Repositories;

public class CourseJobRepository(DbSet<CourseJob> dbSet) : GenericRepository<CourseJob>(dbSet), ICourseJobRepository
{
    public async Task AddRangeAsync(List<CourseJob> courseJobs, CancellationToken cancellationToken)
    {
        await dbSet.AddRangeAsync(courseJobs, cancellationToken);
    }

    public async Task<IEnumerable<CourseJobStatusResponse>> GetJobStatusesAsync(Guid[] jobIds,
        CancellationToken cancellationToken)
    {
        return await dbSet
            .Where(j => jobIds.Contains(j.Id))
            .AsNoTracking()
            .Select(j => new CourseJobStatusResponse(
                j.Id, j.Status.ToString(), j.CourseId, j.ErrorMessage))
            .ToListAsync(cancellationToken);
    }
}