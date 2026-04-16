using CareerFlow.Core.Application.CQRS.Accounts.Queries;
using CareerFlow.Core.Domain.Abstractions.Services;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handlers;

public class LinkedinMobileLoginQueryHandler
{
    private readonly ISocialService _socialService;

    public LinkedinMobileLoginQueryHandler(ISocialService socialService)
    {
        ArgumentNullException.ThrowIfNull(socialService);
        _socialService = socialService;
    }

    public async Task<string> Handle(LinkedinMobileLoginQuery request, CancellationToken cancellationToken)
    {
        return await _socialService.LinkedInMobileLogin(request.ReturnUrl);
    }
}