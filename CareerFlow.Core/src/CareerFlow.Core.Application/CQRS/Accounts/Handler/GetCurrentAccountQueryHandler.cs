using System.Text.Json;
using CareerFlow.Core.Application.CQRS.Accounts.Query;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handler;

public class GetCurrentAccountQueryHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<GetCurrentAccountQueryHandler> _logger;

    public GetCurrentAccountQueryHandler(IAccountRepository accountRepository,
        ILogger<GetCurrentAccountQueryHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(accountRepository);
        ArgumentNullException.ThrowIfNull(logger);
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<AccountDto> Handle(GetCurrentAccountQuery request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account is null)
        {
            _logger.LogError("Contul cu id-ul {AccountId} nu a fost gasit", request.AccountId);
            throw new AccountNotFoundException($"Contul cu id-ul '{request.AccountId}' nu a fost gasit");
        }

        var accountDto = account.ToAccountDto();
        _logger.LogInformation("Contul curent: {AccountDto}",
            JsonSerializer.Serialize(accountDto, new JsonSerializerOptions { WriteIndented = true }));
        return accountDto;
    }
}