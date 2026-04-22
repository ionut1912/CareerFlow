using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infra.Services;

namespace CareerFlow.Core.Infrastructure.Persistence.Repositories;

public class CourseJobRepository(DbSet<CourseJob> courseJobs)
    : GenericRepository<CourseJob>(courseJobs), ICourseJobRepository
{
    private readonly DbSet<CourseJob> _courseJobs = courseJobs;

    public async Task AddRangeAsync(List<CourseJob> courseJobs, CancellationToken cancellationToken) => await _courseJobs.AddRangeAsync(courseJobs, cancellationToken);
}
