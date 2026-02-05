using CareerFlow.Core.Application.Mediatr.Accounts.Commands;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Application.Mediator;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Application.Mediatr.Accounts.Handlers;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly ILogger<ResetPasswordCommandHandler> _logger;
    private readonly IAccountRepository _accountRepository;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordCommandHandler(ILogger<ResetPasswordCommandHandler> logger,IAccountRepository accountRepository,IPasswordService passwordService,IUnitOfWork unitOfWork)
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

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetAccountByUsernameAsync(request.Username, cancellationToken);
        if (account is null)
        {
            _logger.LogError("Account with username {Uswrname} was not found", request.Username);
            throw new AccountNotFoundException($"Account with username '{request.Username}' was not found.");
        }

        account.ResetPassword(request.NewPassword, _passwordService);
        _accountRepository.Update(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Password for account with username {Username} has been reset", request.Username);
        return Unit.Value;
    }
}
