using CareerFlow.Core.Application.CQRS.Accounts.Command;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handler;

public class AcceptLegalDocCommandHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AcceptLegalDocCommandHandler> _logger;


    public AcceptLegalDocCommandHandler(IAccountRepository accountRepository, IUnitOfWork unitOfWork, ILogger<AcceptLegalDocCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(accountRepository);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(logger);
        
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(AcceptLegalDocCommand request, CancellationToken cancellationToken)
    {
        var account=await _accountRepository.GetByIdAsync(request.AccountId,cancellationToken);
        if (account == null)
        {
            _logger.LogError("Account with Id {Id} does not exist", request.AccountId);
            throw new AccountNotFoundException($"Countul cu id-ul {request.AccountId} nu a fost gasit");
        }

        switch (request.Type)
        {
            case "Terms":
                account.AcceptTerms();
                break;
            case "Privacy":
                account.AcceptPrivacyPolicy();
                break;
            default:
                throw new LegalDocInvalidTypeException("Invalid legal document type");
            
        }
        
        _accountRepository.Update(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}