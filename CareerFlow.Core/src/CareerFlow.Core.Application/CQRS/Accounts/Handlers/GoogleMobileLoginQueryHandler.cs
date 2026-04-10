using CareerFlow.Core.Application.CQRS.Accounts.Queries;
using CareerFlow.Core.Domain.Abstractions.Services;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handlers;

public class GoogleMobileLoginQueryHandler
{
    private readonly ISocialService _socialService;

    public GoogleMobileLoginQueryHandler(ISocialService socialService)
    {
        ArgumentNullException.ThrowIfNull(socialService);
        _socialService = socialService;
    }

    public async  Task<string> Handle(GoogleMobileLoginQuery request, CancellationToken cancellationToken)
    {
        return  await Task.FromResult( _socialService.GoogleMobileLogin(request.ReturnUrl));
    }
}