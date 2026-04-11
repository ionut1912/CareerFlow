using CareerFlow.Core.Application.CQRS.Accounts.Queries;
using CareerFlow.Core.Domain.Abstractions.Services;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handlers;

public class GoogleMobileCallbackQueryHandler
{
<<<<<<< HEAD
    private readonly ISocialService  _socialService;

    public GoogleMobileCallbackQueryHandler(ISocialService socialService)
    {
        ArgumentNullException.ThrowIfNull(socialService,nameof(socialService));
=======
    private readonly ISocialService _socialService;

    public GoogleMobileCallbackQueryHandler(ISocialService socialService)
    {
        ArgumentNullException.ThrowIfNull(socialService);
>>>>>>> master
        _socialService = socialService;
    }

    public async Task<string> Handle(GoogleMobileCallbackQuery request, CancellationToken cancellationToken)
    {
<<<<<<< HEAD
        return await _socialService.GoogleMobileCallBackAsync(request.Code,request.State,cancellationToken);
=======
        return await _socialService.GoogleMobileCallBackAsync(request.Code, request.State, cancellationToken);
>>>>>>> master
    }
}