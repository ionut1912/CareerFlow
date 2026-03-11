using CareerFlow.Core.Domain.Models.OpenAi;

namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface IAICompletionService
{
    Task<CompletionResult> CompleteAsync(
        CompletionRequest request,
        CancellationToken ct = default);
}