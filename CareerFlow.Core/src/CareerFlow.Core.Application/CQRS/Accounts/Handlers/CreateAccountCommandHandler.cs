using CareerFlow.Core.Application.CQRS.Accounts.Commands;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handlers;

public class CreateAccountCommandHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<CreateAccountCommandHandler> _logger;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAccountCommandHandler(IAccountRepository accountRepository,
        IPasswordService passwordService, IUnitOfWork unitOfWork, ILogger<CreateAccountCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(accountRepository);
        ArgumentNullException.ThrowIfNull(passwordService);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(logger);
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
            _logger.LogError("Contul cu email :{Email} nu poate fi creat,deoarece exista", request.Email);
            throw new UserAlreadyExistsException($"Contul cu email {request.Email} deja exista");
        }

        if (request.Password != request.ConfirmPassword)
        {
            _logger.LogError("Nu putem crea parola pentru ca parola si parola de confirmare nu corespund");
            throw new PasswordNotMatchException("Parolele nu corespund");
        }

        var accountToCreate = Account.Create(request.Email, request.Password, request.Username, request.Name);
        accountToCreate.AcceptPrivacyPolicy();
        accountToCreate.AcceptTerms();
        accountToCreate.HashPassword(_passwordService);
        await _accountRepository.AddAsync(accountToCreate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Contul cu  id-ul {Id} a fost creat cu succes", accountToCreate.Id);
        return accountToCreate.Id;
    }
}