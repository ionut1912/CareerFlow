using CareerFlow.Core.Application.CQRS.Accounts.Queries;
using CareerFlow.Core.Domain.Abstractions.Services;

using JetBrains.Annotations;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handlers;

[UsedImplicitly]
public class GoogleMobileLoginQueryHandler
{
    private readonly ISocialService _socialService;

    public GoogleMobileLoginQueryHandler(ISocialService socialService)
    {
        ArgumentNullException.ThrowIfNull(socialService);
        _socialService = socialService;
    }

    [UsedImplicitly]
    public async Task<string> Handle(GoogleMobileLoginQuery request, CancellationToken cancellationToken) =>
        await _socialService.GoogleMobileLogin(request.ReturnUrl);
}
