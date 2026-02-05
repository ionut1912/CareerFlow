using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Application.Mediatr.Accounts.Commands;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Application.Mediator;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Application.Mediatr.Accounts.Handlers;

public class AcceptPrivacyPolicyCommandHandler : IRequestHandler<AcceptPrivacyPolicyCommand, AccountDto>
{
    private readonly ILogger<AcceptPrivacyPolicyCommandHandler> _logger;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AcceptPrivacyPolicyCommandHandler(ILogger<AcceptPrivacyPolicyCommandHandler> logger, IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(accountRepository, nameof(accountRepository));
        ArgumentNullException.ThrowIfNull(unitOfWork, nameof(unitOfWork));
        _logger = logger;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }


    public async Task<AccountDto> Handle(AcceptPrivacyPolicyCommand request, CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByIdAsync(request.Id, cancellationToken);
        if (account is null)
        {
            _logger.LogWarning("Account with Id {Id} not found.", request.Id);
            throw new AccountNotFoundException($"Account with Id {request.Id} not found.");
        }

        account.AcceptPrivacyPolicy();
        _accountRepository.Update(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Privacy policy for user with id {Id} accepted.", request.Id);
        return account.ToAccountDto(null);
    }
}
