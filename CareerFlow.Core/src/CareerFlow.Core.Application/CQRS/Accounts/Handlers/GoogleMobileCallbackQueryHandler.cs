using CareerFlow.Core.Application.CQRS.Accounts.Queries;
using CareerFlow.Core.Domain.Abstractions.Services;
using JetBrains.Annotations;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handlers;

[UsedImplicitly]
public class GoogleMobileCallbackQueryHandler
{
    private readonly ISocialService _socialService;

    public GoogleMobileCallbackQueryHandler(ISocialService socialService)
    {
        ArgumentNullException.ThrowIfNull(socialService);

        _socialService = socialService;
    }

    [UsedImplicitly]
    public async Task<string> Handle(GoogleMobileCallbackQuery request, CancellationToken cancellationToken) => await _socialService.GoogleMobileCallBackAsync(request.Code, request.State, cancellationToken);
}
