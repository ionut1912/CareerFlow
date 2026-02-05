using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Application.Mediatr.Accounts.Query;
using CareerFlow.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Application.Mediator;
using System.Text.Json;

namespace CareerFlow.Core.Application.Mediatr.Accounts.Handlers;

public class GetAllAcountsQueryHandler : IRequestHandler<GetAllAcountsQuery, List<AccountDto>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<GetAllAcountsQueryHandler> _logger;

    public GetAllAcountsQueryHandler(IAccountRepository accountRepository, ILogger<GetAllAcountsQueryHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(accountRepository, nameof(accountRepository));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<List<AccountDto>> Handle(GetAllAcountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _accountRepository.GetAllAsync(cancellationToken);
        var accountDtos = accounts.ToAccountsDto();
        _logger.LogInformation("Retrieved {accountDtos}  from the repository.", JsonSerializer.Serialize(accountDtos, new JsonSerializerOptions { WriteIndented = true }));
        return accountDtos;
    }
}
