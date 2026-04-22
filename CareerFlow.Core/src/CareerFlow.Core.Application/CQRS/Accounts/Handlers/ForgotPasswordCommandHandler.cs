using CareerFlow.Core.Application.CQRS.Accounts.Commands;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Rabbit.Events.Events;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handlers;

[UsedImplicitly]
public partial class ForgotPasswordCommandHandler(
    IAccountRepository accountRepository,
    ILogger<ForgotPasswordCommandHandler> logger,
    IPasswordService passwordService,
    IUnitOfWork unitOfWork)
{
    [UsedImplicitly]
    public async Task<OutgoingMessages> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        Account? account = await accountRepository.GetAccountByEmailAsync(request.Email, cancellationToken);
        if (account is null)
        {
            LogAccountNotFound(logger, request.Email);
            throw new AccountNotFoundException($"Contul cu email-ul '{request.Email}' nu a fost gasit.");
        }

        account.GenerateResetPasswordToken(request.Token, passwordService);
        accountRepository.Update(account);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var messages = new OutgoingMessages
        {
            new ResetPasswordNotificationMessage(account.Username, account.Email, request.ResetPasswordLink)
        };

        LogResetPasswordEmailSent(logger);
        return messages;
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "User-ul cu email-ul {Email} nu a fost gasit")]
    private static partial void LogAccountNotFound(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Information, Message = "Email-ul de resetare parola a fost resetata")]
    private static partial void LogResetPasswordEmailSent(ILogger logger);
}
