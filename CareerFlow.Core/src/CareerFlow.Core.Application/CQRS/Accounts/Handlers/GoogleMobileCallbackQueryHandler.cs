using CareerFlow.Core.Application.CQRS.Accounts.Queries;
using CareerFlow.Core.Domain.Abstractions.Services;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handlers;

public class GoogleMobileCallbackQueryHandler
{
    private readonly ISocialService _socialService;

    public GoogleMobileCallbackQueryHandler(ISocialService socialService)
    {
        ArgumentNullException.ThrowIfNull(socialService);
        _socialService = socialService;
    }

    public async Task<string> Handle(GoogleMobileCallbackQuery request, CancellationToken cancellationToken)
    {
        return await _socialService.GoogleMobileCallBackAsync(request.Code, request.State, cancellationToken);
    }
}