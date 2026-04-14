using CareerFlow.Core.Domain.Models.Assembly;

namespace CareerFlow.Core.Domain.Abstractions.Services;
public interface ICoursePersistenceService
{
    Task<Guid> PersistAsync(Guid userId, string topic, List<ChapterAssemblyModel> assemblyData, CancellationToken ct = default);
}