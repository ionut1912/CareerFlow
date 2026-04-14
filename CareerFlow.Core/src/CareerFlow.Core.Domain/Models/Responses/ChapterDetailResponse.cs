using System.Text.Json.Serialization;
using CareerFlow.Core.Domain.Models.AI.Dto;

namespace CareerFlow.Core.Domain.Models.Responses;

public sealed record ChapterDetailResponse(
    [property: JsonPropertyName("subchapters")] List<DetailedSubchapterDto> Subchapters,
    [property: JsonPropertyName("recap_quiz")] List<QuestionDto> RecapQuiz);