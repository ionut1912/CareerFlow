using CareerFlow.Core.Domain.Entities;

using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Domain.Abstractions.Repositories;

public interface ICourseUploadsRepository : IGenericRepository<CourseUpload>
{
    Task AddRangeAsync(List<CourseUpload> courseUploads, CancellationToken cancellationToken);
}
