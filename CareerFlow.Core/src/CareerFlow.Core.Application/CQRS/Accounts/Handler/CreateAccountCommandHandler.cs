using CareerFlow.Core.Application.CQRS.Accounts.Command;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handler;

public class CreateAccountCommandHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateAccountCommandHandler> _logger;

    public CreateAccountCommandHandler(IAccountRepository accountRepository,
        IPasswordService passwordService, IUnitOfWork unitOfWork, ILogger<CreateAccountCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(accountRepository, nameof(accountRepository));
        ArgumentNullException.ThrowIfNull(passwordService, nameof(passwordService));
        ArgumentNullException.ThrowIfNull(unitOfWork, nameof(unitOfWork));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        _accountRepository = accountRepository;
        _passwordService = passwordService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetAccountByEmailAsync(request.Email, cancellationToken);
        if (account is not null)
        {
            _logger.LogError("Accont with username :{Username} can not be created because already exists", request.Username);
            throw new UserAlreadyExistsException($"Account with username {request.Username} already exists");
        }

        var accountToCreate = Account.Create(request.Email, request.Password, request.Username,request.Name);

        if (request.AcceptedPrivacyPolicy)
        {
            accountToCreate.AcceptPrivacyPolicy();
        }

        if (request.AcceptedTermsAndConditions)
        {
            accountToCreate.AcceptTerms();
        }

        accountToCreate.HashPassword(_passwordService);
        await _accountRepository.AddAsync(accountToCreate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Account with id {Id} was created", accountToCreate.Id);
        return accountToCreate.Id;
    }
}