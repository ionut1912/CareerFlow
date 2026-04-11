using CareerFlow.Core.Application.CQRS.Accounts.Queries;
using CareerFlow.Core.Domain.Abstractions.Services;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handlers;

public class LinkedInMobileCallbackQueryHandler
{
    private readonly ISocialService _socialService;

    public LinkedInMobileCallbackQueryHandler(ISocialService socialService)
    {
<<<<<<< HEAD
        ArgumentNullException.ThrowIfNull(socialService, nameof(socialService));
=======
        ArgumentNullException.ThrowIfNull(socialService);
>>>>>>> master
        _socialService = socialService;
    }

    public async Task<string> Handle(LinkedInMobileCallbackQuery request, CancellationToken cancellationToken)
    {
<<<<<<< HEAD
        return await _socialService.LinkedInCallBackAsync(request.Code,request.State,cancellationToken);
=======
        return await _socialService.LinkedInCallBackAsync(request.Code, request.State, cancellationToken);
>>>>>>> master
    }
}