using CareerFlow.Core.Application.CQRS.Accounts.Command;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Rabbit.Events.Events;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;
using Wolverine;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handler;

public class ForgotPasswordCommandHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;

    public ForgotPasswordCommandHandler(IAccountRepository accountRepository,
        ILogger<ForgotPasswordCommandHandler> logger, IPasswordService passwordService,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _logger = logger;
        _passwordService = passwordService;
        _unitOfWork = unitOfWork;
    }

    public async Task<OutgoingMessages> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetAccountByEmailAsync(request.Email, cancellationToken);
        if (account is null)
        {
            _logger.LogError("User-ul cu email-ul {Email} nu a fost gasit", request.Email);
            throw new AccountNotFoundException($"Contul cu email-ul '{request.Email}' nu a fost gasit.");
        }

        account.GenerateResetPasswordToken(request.Token, _passwordService);
        _accountRepository.Update(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var messages = new OutgoingMessages
        {
            new ResetPasswordNotificationMessage(account.Username, account.Email, request.ResetPasswordLink)
        };

        _logger.LogInformation("Email-ul de resetare parola a fost resetata");
        return messages;
    }
}