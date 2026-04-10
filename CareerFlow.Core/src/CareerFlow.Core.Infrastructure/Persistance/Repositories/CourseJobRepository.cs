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
}