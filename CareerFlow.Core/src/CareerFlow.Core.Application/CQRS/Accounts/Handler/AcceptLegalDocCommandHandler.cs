using CareerFlow.Core.Application.CQRS.Accounts.Command;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handler;

public class AcceptLegalDocCommandHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<AcceptLegalDocCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;


    public AcceptLegalDocCommandHandler(IAccountRepository accountRepository, IUnitOfWork unitOfWork,
        ILogger<AcceptLegalDocCommandHandler> logger)
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
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account == null)
        {
            _logger.LogError("Contul cu  {Id} nu exista", request.AccountId);
            throw new AccountNotFoundException($"Contul cu id-ul {request.AccountId} nu a fost gasit");
        }

        var normalizedType = request.Type?.Trim();
        switch (normalizedType)
        {
            case var t when string.Equals(t, "Terms", StringComparison.OrdinalIgnoreCase):
                account.AcceptTerms();
                break;
            case var t when string.Equals(t, "Privacy", StringComparison.OrdinalIgnoreCase):
                account.AcceptPrivacyPolicy();
                break;
            default:
                throw new LegalDocInvalidTypeException("Invalid legal document type");
        }

        _accountRepository.Update(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}