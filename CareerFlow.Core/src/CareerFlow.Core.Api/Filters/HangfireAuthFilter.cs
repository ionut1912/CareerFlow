using Hangfire.Dashboard;

namespace CareerFlow.Core.Api.Filters;

public sealed class HangfireAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        HttpContext? http = context.GetHttpContext();
        
        return http.User.Identity?.IsAuthenticated == true;
    }
}
