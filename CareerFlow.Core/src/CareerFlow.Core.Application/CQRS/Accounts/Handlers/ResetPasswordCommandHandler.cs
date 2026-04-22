using CareerFlow.Core.Application.CQRS.Accounts.Commands;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handlers;

public partial class ResetPasswordCommandHandler
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
        Account? account = await _accountRepository.GetAccountByEmailAsync(request.Email, cancellationToken);
        if (account is null)
        {
            LogAccountNotFound(_logger, request.Email);
            throw new AccountNotFoundException($"Contul cu Email-ul '{request.Email}' nu a fost gasit.");
        }

        if (!_passwordService.VerifyPassword(request.Token, account.ResetPasswordToken))
        {
            LogTokensNotMatching(_logger);
            throw new PasswordNotMatchException("Tokenurile nu sunt la fel");
        }

        if (account.ResetPasswordTokenExpiresAt <= DateTime.UtcNow)
        {
            LogTokenExpired(_logger);
            throw new InvalidFieldException("Tokenul e expirat");
        }

        account.ResetPasswordTokenAndExpiry();
        account.ResetPassword(request.NewPassword, _passwordService);
        _accountRepository.Update(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        LogPasswordReset(_logger, request.Email);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "User-ul cu Email-ul {Email} nu a fost gasit")]
    private static partial void LogAccountNotFound(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Error, Message = "Tokenurile nu sunt la fel")]
    private static partial void LogTokensNotMatching(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "Tokenul e expirat")]
    private static partial void LogTokenExpired(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Parola pentru contul cu email-ul {Email} a fost resetata")]
    private static partial void LogPasswordReset(ILogger logger, string email);
}
