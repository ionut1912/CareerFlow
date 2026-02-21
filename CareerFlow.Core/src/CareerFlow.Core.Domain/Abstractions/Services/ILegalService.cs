using CareerFlow.Core.Domain.Models;

namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface ILegalService
{
    Task<LegalDocumentResponse?> GetDocumentAsync(string type, CancellationToken cancellationToken);
}