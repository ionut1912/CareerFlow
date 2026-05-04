using CareerFlow.Core.Application.CQRS.Accounts.Commands;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;

using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handlers;

public partial class DeleteAccountCommandHandler
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
        Account? account = await _accountRepository.GetByIdAsync(request.Id, cancellationToken);
        if (account is null)
        {
            LogAccountNotFound(request.Id);
            throw new AccountNotFoundException($"User-ul cu Id-ul {request.Id} nu a fost gasit.");
        }

        _accountRepository.Delete(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        LogAccountDeleted(request.Id);
    }

    [LoggerMessage(Level = LogLevel.Error,
        Message = "User-ul cu id-ul : {Id} nu poate fi sters,deoarce nu a fost gasit")]
    private partial void LogAccountNotFound(Guid id);

    [LoggerMessage(Level = LogLevel.Information, Message = "User-ul cu id-ul: {Id} a fost sters cu succes")]
    private partial void LogAccountDeleted(Guid id);
}
