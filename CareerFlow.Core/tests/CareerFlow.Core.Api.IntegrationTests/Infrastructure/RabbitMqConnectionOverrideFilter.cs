using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace CareerFlow.Core.Api.IntegrationTests.Infrastructure;

internal sealed class RabbitMqConnectionOverrideFilter : IStartupFilter
{
    private readonly string _connectionString;
    public RabbitMqConnectionOverrideFilter(string connectionString)
        => _connectionString = connectionString;

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        => app => next(app);
}