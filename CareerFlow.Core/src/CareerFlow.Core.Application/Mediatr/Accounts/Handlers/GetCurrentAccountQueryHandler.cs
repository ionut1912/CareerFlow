using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Application.Mediatr.Accounts.Query;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Shared.Application.Mediator;
using System.Text.Json;

namespace CareerFlow.Core.Application.Mediatr.Accounts.Handlers;

public class GetCurrentAccountQueryHandler : IRequestHandler<GetCurrentAccountQuery, AccountDto>
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
        var account = await _accountRepository.GetAccountByUsernameAsync(request.Username, cancellationToken);
        if (account is null)
        {
            _logger.LogError("Account with username {Uswrname} was not found", request.Username);
            throw new AccountNotFoundException($"Account with username '{request.Username}' was not found.");
        }

        var accountDto = account.ToAccountDto(null);
        _logger.LogInformation("Current Account: {AccountDto}",
            JsonSerializer.Serialize(accountDto, new JsonSerializerOptions { WriteIndented = true }));
        return accountDto;
    }
}