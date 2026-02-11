using CareerFlow.Core.Application.CQRS.Accounts.Command;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handler;

public class ResetPasswordCommandHandler
{
    private readonly ILogger<ResetPasswordCommandHandler> _logger;
    private readonly IAccountRepository _accountRepository;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordCommandHandler(ILogger<ResetPasswordCommandHandler> logger, IAccountRepository accountRepository, IPasswordService passwordService, IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(accountRepository, nameof(accountRepository));
        ArgumentNullException.ThrowIfNull(passwordService, nameof(passwordService));
        ArgumentNullException.ThrowIfNull(unitOfWork, nameof(unitOfWork));
        _logger = logger;
        _accountRepository = accountRepository;
        _passwordService = passwordService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account is null)
        {
            _logger.LogError("Account with Id {AccountId} was not found", request.AccountId);
            throw new AccountNotFoundException($"Account with Id'{request.AccountId}' was not found.");
        }

        account.ResetPassword(request.NewPassword, _passwordService);
        _accountRepository.Update(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Password for account with AccountId {AccountId} has been reset", request.AccountId);

    }
}
