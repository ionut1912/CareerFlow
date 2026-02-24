using CareerFlow.Core.Application.CQRS.Accounts.Command;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Rabbit.Events.Events;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;
using Wolverine;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handler;

public class ResetPasswordCommandHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordCommandHandler(ILogger<ResetPasswordCommandHandler> logger,
        IAccountRepository accountRepository, IPasswordService passwordService, IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(accountRepository);
        ArgumentNullException.ThrowIfNull(passwordService);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _logger = logger;
        _accountRepository = accountRepository;
        _passwordService = passwordService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetAccountByEmailAsync(request.Email, cancellationToken);
        if (account is null)
        {
            _logger.LogError("User-ul cu Email-ul {Email} nu a fost gasit", request.Email);
            throw new AccountNotFoundException($"Contul cu Email-ul '{request.Email}' nu a fost gasit.");
        }

        if (!_passwordService.VerifyPassword(request.Token, account.ResetPasswordToken))
        {
            _logger.LogError("Tokenurile nu sunt la fel");
            throw new PasswordNotMatchException("Tokenurile nu sunt la fel");
        }

        if (account.ResetPasswordTokenExpiresAt <= DateTime.UtcNow)
        {
            _logger.LogError("Tokenul e expirat");
            throw new InvalidFieldException("Tokenul e expirat");
        }
        
        account.ResetPasswordTokenAndExpiry();
        account.ResetPassword(request.NewPassword, _passwordService);
        _accountRepository.Update(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Parola pentru contul cu email-ul {Email} a fost resetata", request.Email);

    }
}