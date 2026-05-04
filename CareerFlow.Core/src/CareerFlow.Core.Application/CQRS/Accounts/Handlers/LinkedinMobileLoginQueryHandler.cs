using CareerFlow.Core.Application.CQRS.Accounts.Queries;
using CareerFlow.Core.Domain.Abstractions.Services;

using JetBrains.Annotations;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handlers;

[UsedImplicitly]
public class LinkedinMobileLoginQueryHandler
{
    private readonly ISocialService _socialService;

    public LinkedinMobileLoginQueryHandler(ISocialService socialService)
    {
        ArgumentNullException.ThrowIfNull(socialService);
        _socialService = socialService;
    }

    [UsedImplicitly]
    public async Task<string> Handle(LinkedinMobileLoginQuery request, CancellationToken cancellationToken) =>
        await _socialService.LinkedInMobileLogin(request.ReturnUrl);
}
