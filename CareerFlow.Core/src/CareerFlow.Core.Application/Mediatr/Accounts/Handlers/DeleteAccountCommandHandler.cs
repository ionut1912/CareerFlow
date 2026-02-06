using CareerFlow.Core.Application.Mediatr.Accounts.Commands;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Shared.Application.Mediator;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Application.Mediatr.Accounts.Handlers;

public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, Unit>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteAccountCommandHandler> _logger;

    public DeleteAccountCommandHandler(IAccountRepository accountRepository, IUnitOfWork unitOfWork, ILogger<DeleteAccountCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(accountRepository, nameof(accountRepository));
        ArgumentNullException.ThrowIfNull(unitOfWork, nameof(unitOfWork));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.Id);
        if (account == null)
        {
            _logger.LogError("Account with id : {Id} can not be deleted because is not found", request.Id);
            throw new AccountNotFoundException($"Account with Id {request.Id} was not found.");
        }
        _accountRepository.Delete(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Account with id: {Id} was deleted successfully", request.Id);
        return Unit.Value;
    }
}