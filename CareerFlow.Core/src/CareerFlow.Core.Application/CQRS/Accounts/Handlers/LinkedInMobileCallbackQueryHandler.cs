using CareerFlow.Core.Application.CQRS.Accounts.Queries;
using CareerFlow.Core.Domain.Abstractions.Services;

using JetBrains.Annotations;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handlers;

[UsedImplicitly]
public class LinkedInMobileCallbackQueryHandler
{
    private readonly ISocialService _socialService;

    public LinkedInMobileCallbackQueryHandler(ISocialService socialService)
    {
        ArgumentNullException.ThrowIfNull(socialService);
        _socialService = socialService;
    }

    [UsedImplicitly]
    public async Task<string> Handle(LinkedInMobileCallbackQuery request, CancellationToken cancellationToken) =>
        await _socialService.LinkedInCallBackAsync(request.Code, request.State, cancellationToken);
}
