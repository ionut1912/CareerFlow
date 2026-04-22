using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infra.Services;

namespace CareerFlow.Core.Infrastructure.Persistence.Repositories;

public class CourseUploadsRepository(DbSet<CourseUpload> uploads)
    : GenericRepository<CourseUpload>(uploads), ICourseUploadsRepository
{
    private readonly DbSet<CourseUpload> _uploads = uploads;

    public async Task AddRangeAsync(List<CourseUpload> courseUploads, CancellationToken cancellationToken) => await _uploads.AddRangeAsync(courseUploads, cancellationToken);
}
