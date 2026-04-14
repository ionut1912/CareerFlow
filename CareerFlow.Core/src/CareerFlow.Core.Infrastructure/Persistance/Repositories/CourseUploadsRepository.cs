using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infra.Services;

namespace CareerFlow.Core.Infrastructure.Persistance.Repositories;

public class CourseUploadsRepository(DbSet<CourseUpload> dbSet)
    : GenericRepository<CourseUpload>(dbSet), ICourseUploadsRepository
{
    public async Task AddRangeAsync(List<CourseUpload> courseUploads, CancellationToken cancellationToken)
    {
        await dbSet.AddRangeAsync(courseUploads, cancellationToken);
    }
}