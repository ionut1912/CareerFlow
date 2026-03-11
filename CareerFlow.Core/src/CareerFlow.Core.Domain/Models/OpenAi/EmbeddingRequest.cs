namespace CareerFlow.Core.Domain.Models.OpenAi;

public sealed record EmbeddingRequest(
    string Input,
    string Model
);