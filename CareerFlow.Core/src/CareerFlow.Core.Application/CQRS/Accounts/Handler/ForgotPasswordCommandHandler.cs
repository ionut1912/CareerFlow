using CareerFlow.Core.Application.CQRS.Accounts.Command;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Rabbit.Events.Events;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handler;

public class ForgotPasswordCommandHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;


    public ForgotPasswordCommandHandler(IAccountRepository accountRepository,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<OutgoingMessages> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetAccountByEmailAsync(request.Email, cancellationToken);
        if (account is null)
        {
            _logger.LogError("User-ul cu email-ul {Email} nu a fost gasit", request.Email);
            throw new AccountNotFoundException($"Contul cu email-ul '{request.Email}' nu a fost gasit.");
        }
        
        var messages = new OutgoingMessages
        {
            new ResetPasswordNotificationMessage(account.Username, account.Email, request.ResetPasswordLink)
        };

        _logger.LogInformation("Email-ul de resetare parola a fost resetata");
        return messages;
    }
}