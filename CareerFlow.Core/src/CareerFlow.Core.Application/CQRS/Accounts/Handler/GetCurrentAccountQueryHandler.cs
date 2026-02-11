using CareerFlow.Core.Application.CQRS.Accounts.Query;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handlers;

public class GetCurrentAccountQueryHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<GetCurrentAccountQueryHandler> _logger;

    public GetCurrentAccountQueryHandler(IAccountRepository accountRepository, ILogger<GetCurrentAccountQueryHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(accountRepository, nameof(accountRepository));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<AccountDto> Handle(GetCurrentAccountQuery request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account is null)
        {
            _logger.LogError("Account with accountId {AccountId} was not found", request.AccountId);
            throw new AccountNotFoundException($"Account with accountId '{request.AccountId}' was not found.");
        }

        var accountDto = account.ToAccountDto(null);
        _logger.LogInformation("Current Account: {AccountDto}",
            JsonSerializer.Serialize(accountDto, new JsonSerializerOptions { WriteIndented = true }));
        return accountDto;
    }
}