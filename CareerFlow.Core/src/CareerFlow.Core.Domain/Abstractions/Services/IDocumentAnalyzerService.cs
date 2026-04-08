using CareerFlow.Core.Domain.Models.AI;
using Microsoft.AspNetCore.Http;

namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface IDocumentAnalyzerService
{
    Task<FullCourseResponse> GenerateFullCourseAsync(IFormFile document, int numberOfDays,
        CancellationToken ct = default);
}