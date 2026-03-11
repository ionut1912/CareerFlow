namespace CareerFlow.Core.Infrastructure.Modles.OpenAi;

public sealed record EmbeddingRequest(
    string Input,
    string Model
);