using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;

namespace CareerFlow.Core.Infrastructure.HangfireJobs;

public class LegalDocumentCheckerJob
{
    private readonly IAccountRepository _accountRepository;
    private readonly IGithubPagesRequestsSender _githubPagesRequestsSender;
    private readonly ISystemDocumentRepository _systemDocumentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LegalDocumentCheckerJob(IGithubPagesRequestsSender githubPagesRequestsSender,
        IAccountRepository accountRepository, IUnitOfWork unitOfWork,
        ISystemDocumentRepository systemDocumentRepository)
    {
        ArgumentNullException.ThrowIfNull(githubPagesRequestsSender);
        ArgumentNullException.ThrowIfNull(accountRepository);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(systemDocumentRepository);

        _githubPagesRequestsSender = githubPagesRequestsSender;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _systemDocumentRepository = systemDocumentRepository;
    }

    public async Task CheckForUpdatesAsync(string documentType, CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await _githubPagesRequestsSender.GetContentAsync(documentType, cancellationToken);
        response.EnsureSuccessStatusCode();

        string? currentEtag = response.Headers.ETag?.Tag;
        if (string.IsNullOrWhiteSpace(currentEtag)) return;

        SystemDocument? documentRecord = await _systemDocumentRepository.FindByTypeAsync(documentType, cancellationToken);
        if (documentRecord == null)
        {
            documentRecord = SystemDocument.Create(documentType, currentEtag);
            await _systemDocumentRepository.AddAsync(documentRecord, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        if (documentRecord.CurrentETag != currentEtag)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                documentRecord.Update(currentEtag);
                _systemDocumentRepository.Update(documentRecord);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _accountRepository.UpdateTermsAsync(documentType, cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
