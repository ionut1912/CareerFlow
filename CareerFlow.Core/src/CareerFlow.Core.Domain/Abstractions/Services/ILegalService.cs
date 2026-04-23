using CareerFlow.Core.Domain.Models.Legal;

namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface ILegalService
{
    Task<LegalDocumentResponse?> GetDocumentAsync(string type, CancellationToken cancellationToken);
}
