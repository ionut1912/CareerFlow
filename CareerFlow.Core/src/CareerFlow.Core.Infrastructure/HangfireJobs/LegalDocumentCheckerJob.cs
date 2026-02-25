using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Infrastructure.HangfireJobs;

public class LegalDocumentCheckerJob
{
    private readonly IGithubPagesRequestsSender _githubPagesRequestsSender;
    private readonly ApplicationDbContext _dbContext;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;


    public LegalDocumentCheckerJob(IGithubPagesRequestsSender githubPagesRequestsSender, ApplicationDbContext dbContext, IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(githubPagesRequestsSender);
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(accountRepository);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        
        _githubPagesRequestsSender = githubPagesRequestsSender;
        _dbContext = dbContext;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task CheckForUpdatesAsync(string documentType,CancellationToken cancellationToken = default)
    {
        var response=await _githubPagesRequestsSender.GetContentAsync(documentType,cancellationToken);
        response.EnsureSuccessStatusCode();
        var currentEtag = response.Headers.ETag?.Tag;
        if(string.IsNullOrWhiteSpace(currentEtag)) return;
        var documentRecord =
            await _dbContext.SystemDocuments.FirstOrDefaultAsync(d => d.DocumentType == documentType,
                cancellationToken);
        if (documentRecord == null)
        {
            documentRecord = SystemDocument.Create(documentType, currentEtag);
            _dbContext.SystemDocuments.Add(documentRecord);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        if (documentRecord.CurrentETag != currentEtag)
        {
            documentRecord.Update(currentEtag);
            _dbContext.SystemDocuments.Update(documentRecord);
            await _accountRepository.UpdateTermsAsync(documentType);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

    }
}