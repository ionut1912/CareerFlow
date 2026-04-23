using CareerFlow.Core.Domain.Entities;

using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Domain.Abstractions.Repositories;

public interface ICourseJobRepository : IGenericRepository<CourseJob>
{
    Task AddRangeAsync(List<CourseJob> courseJobs, CancellationToken cancellationToken);
}
