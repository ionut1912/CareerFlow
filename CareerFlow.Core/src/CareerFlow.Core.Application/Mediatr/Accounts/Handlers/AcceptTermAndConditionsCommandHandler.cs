using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Application.Mediatr.Accounts.Commands;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Shared.Application.Mediator;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Application.Mediatr.Accounts.Handlers;

public class AcceptTermAndConditionsCommandHandler : IRequestHandler<AcceptTermAndConditionsCommand, AccountDto>
{
    private readonly ILogger<AcceptTermAndConditionsCommandHandler> _logger;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AcceptTermAndConditionsCommandHandler(
        ILogger<AcceptTermAndConditionsCommandHandler> logger,
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(accountRepository, nameof(accountRepository));
        ArgumentNullException.ThrowIfNull(unitOfWork, nameof(unitOfWork));
        _logger = logger;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountDto> Handle(AcceptTermAndConditionsCommand request, CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByIdAsync(request.Id, cancellationToken);
        if (account is null)
        {
            _logger.LogWarning("Terms and Conditions with Id {Id} not found.", request.Id);
            throw new TermsAndConditionsNotFoundException($"Terms and Conditions with Id {request.Id} not found.");
        }
        account.AcceptTerms();
        _accountRepository.Update(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Terms and Conditions with Id {Id} accepted.", request.Id);
        return account.ToAccountDto(null);

    }
}
