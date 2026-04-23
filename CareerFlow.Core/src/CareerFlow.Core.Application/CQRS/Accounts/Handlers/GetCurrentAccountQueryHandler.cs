using System.Text.Json;

using CareerFlow.Core.Application.CQRS.Accounts.Queries;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;

using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handlers;

public partial class GetCurrentAccountQueryHandler
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

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
        Account? account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account is null)
        {
            LogAccountNotFound(request.AccountId);
            throw new AccountNotFoundException($"Contul cu id-ul '{request.AccountId}' nu a fost gasit");
        }

        var accountDto = account.ToAccountDto();

        if (_logger.IsEnabled(LogLevel.Information))
            LogCurrentAccount(JsonSerializer.Serialize(accountDto, _jsonOptions));

        return accountDto;
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Contul cu id-ul {AccountId} nu a fost gasit")]
    private partial void LogAccountNotFound(Guid accountId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Contul curent: {AccountDto}")]
    private partial void LogCurrentAccount(string accountDto);
}
