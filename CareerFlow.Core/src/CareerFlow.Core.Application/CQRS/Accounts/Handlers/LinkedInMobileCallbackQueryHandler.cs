using CareerFlow.Core.Application.CQRS.Accounts.Queries;
using CareerFlow.Core.Domain.Abstractions.Services;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handlers;

public class LinkedInMobileCallbackQueryHandler
{
    private readonly ISocialService _socialService;

    public LinkedInMobileCallbackQueryHandler(ISocialService socialService)
    {
        ArgumentNullException.ThrowIfNull(socialService);
        _socialService = socialService;
    }

    public async Task<string> Handle(LinkedInMobileCallbackQuery request, CancellationToken cancellationToken)
    {
        return await _socialService.LinkedInCallBackAsync(request.Code, request.State, cancellationToken);
    }
}