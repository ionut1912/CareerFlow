using Shouldly;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CareerFlow.Core.Api.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly SharedContainerFixture _containers;
    private TestWebAppFactory? _factory;

    protected IntegrationTestBase(SharedContainerFixture containers)
        => _containers = containers;

    protected HttpClient CreateClient()
        => GetOrCreateFactory().CreateAnonymousClient();

    protected HttpClient CreateAuthenticatedClient(Guid accountId)
        => GetOrCreateFactory().CreateAuthenticatedClient(accountId);

    protected static async Task<T> ShouldBeOkWithAsync<T>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe(HttpStatusCode.OK,
            $"Expected 200 OK but got {(int)response.StatusCode}. Body: {body}");
        var result = await response.Content.ReadFromJsonAsync<T>();
        ((object?)result).ShouldNotBeNull();
        return result!;
    }

    protected static async Task ShouldBeNotFoundWithBodyAsync(
        HttpResponseMessage response, string bodyContains)
    {
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound,
            $"Expected 404 but got {(int)response.StatusCode}. Body: {body}");
        body.ShouldContain(bodyContains);
    }

    protected static async Task ShouldBeNotFoundAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound,
            $"Expected 404 but got {(int)response.StatusCode}. Body: {body}");
    }

    protected static void ShouldBeNoContent(HttpResponseMessage response)
        => response.StatusCode.ShouldBe(HttpStatusCode.NoContent,
            $"Expected 204 but got {(int)response.StatusCode}.");

    protected static void ShouldBeUnauthorized(HttpResponseMessage response)
        => response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized,
            $"Expected 401 but got {(int)response.StatusCode}.");

    protected static void ShouldBeBadRequest(HttpResponseMessage response)
        => response.StatusCode.ShouldBe(HttpStatusCode.BadRequest,
            $"Expected 400 but got {(int)response.StatusCode}.");

    protected static void ShouldHaveContentType(HttpResponseMessage response, string mediaType)
        => response.Content.Headers.ContentType?.MediaType
            .ShouldContain(mediaType);

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual async Task DisposeAsync()
    {
        if (_factory is not null)
            await _factory.DisposeAsync();
    }

    private TestWebAppFactory GetOrCreateFactory()
    {
        if (_factory is not null) return _factory;

        var (host, user, pass, port) = ParseRabbitMqConnectionString(_containers.RabbitMqConnectionString);

        _factory = new TestWebAppFactory()
            .WithPostgres(_containers.PostgresConnectionString)
            .WithRedis(_containers.RedisConnectionString)
            .WithRabbitMq(host, user, pass, port);

        return _factory;
    }

    private static (string Host, string User, string Pass, int Port) ParseRabbitMqConnectionString(
        string connectionString)
    {
        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':');
        var user = userInfo.Length > 0 ? userInfo[0] : "guest";
        var pass = userInfo.Length > 1 ? userInfo[1] : "guest";
        return (uri.Host, user, pass, uri.Port);
    }
}