using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Rabbit.Events.Events;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Application.Messages;

public class UpdateLegalDocsMessageHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<UpdateLegalDocsMessageHandler> _logger;

    public UpdateLegalDocsMessageHandler(IAccountRepository accountRepository, ILogger<UpdateLegalDocsMessageHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(accountRepository, nameof(accountRepository));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task Handle(UpdateLegalDocsMessage message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting bulk update of legal document acceptance for all users.");

        await _accountRepository.ResetLegalAgreementsAsync(cancellationToken);

        _logger.LogInformation("Bulk update completed successfully.");
    }
}
