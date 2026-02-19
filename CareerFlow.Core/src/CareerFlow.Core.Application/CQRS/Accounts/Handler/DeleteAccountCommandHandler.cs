using CareerFlow.Core.Application.CQRS.Accounts.Command;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handler;

public class DeleteAccountCommandHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<DeleteAccountCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAccountCommandHandler(IAccountRepository accountRepository, IUnitOfWork unitOfWork,
        ILogger<DeleteAccountCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(accountRepository);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(logger);
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.Id, cancellationToken);
        if (account is null)
        {
            _logger.LogError("User-ul cu id-ul : {Id} nu poate fi sters,deoarce nu a fost gasit", request.Id);
            throw new AccountNotFoundException($"User-ul cu Id-ul {request.Id} nu a fost gasit.");
        }

        _accountRepository.Delete(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("User-ul cu id-ul: {Id} a fost sters cu succes", request.Id);
    }
}