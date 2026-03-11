using CareerFlow.Core.Infrastructure.Modles.OpenAi;

namespace CareerFlow.Core.Infrastructure.OpenAIAbstractions;

public interface IAICompletionService
{
    Task<CompletionResult> CompleteAsync(
        CompletionRequest request,
        CancellationToken ct = default);
}