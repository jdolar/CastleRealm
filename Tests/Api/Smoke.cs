using Api;
using DataBase.Collections.Castles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Requests;
using Xunit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ApiClient;
using Microsoft.AspNetCore.Mvc.Testing;
using Shared.Tools;
using Shared.Api;
namespace UnitTests.Api;
public class ApiSmokeTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private IRestClient _client;
    private readonly Swagger _swagger;
    private readonly Parser _parser;
    private readonly ILogger<ApiSmokeTests> _logger;
    private static List<Endpoint>? _endpoints = new();
    public ApiSmokeTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<CastleContext>>();
                services.AddDbContext<CastleContext>(options => options.UseSqlServer(SQLServer.DefaultConnection));
                services.AddHttpClient<IRestClient, RestClient>("InternalApi")
                   .ConfigurePrimaryHttpMessageHandler(() => factory.Server.CreateHandler())
                   .ConfigureHttpClient(client =>
                   {
                       client.BaseAddress = new Uri("http://localhost");
                   });
            });
        });

        _client = _factory.Services.GetRequiredService<IRestClient>();
        _logger = _factory.Services.GetRequiredService<ILogger<ApiSmokeTests>>();
        _swagger = new(_client, _logger);
        _parser = new(_logger);
    }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    public async ValueTask InitializeAsync()
    {
        if (_endpoints!.Any())
            return;

        // Run migrations against test DB
        using IServiceScope scope = _factory.Services.CreateScope();
        CastleContext db = scope.ServiceProvider.GetRequiredService<CastleContext>();
        await db.Database.MigrateAsync();

        bool isHealthy = await _swagger.GetStatus();
        if (isHealthy) _endpoints = await _swagger.GetEndPoints()!;
    }

    [Fact]
    public async Task ApiSmoke_AllEndpoints_Success()
    {
        CancellationToken cancellationToken = default;
        if (!_endpoints!.Any())
            return;

        foreach (Endpoint endpoint in _endpoints!)
        {
            Type? type = _parser.GetDtoType(endpoint.Operation);            
            if (type is null) continue;
            
            object? payLoad = _parser.GetPayLoad(type!);
            IResponse? result = endpoint.Method.ToUpperInvariant() switch
            {
                "GET" => await _client.Get<object, IResponse>(endpoint.Path, payLoad!, cancellationToken),
                "POST" => await _client.Post<object, IResponse>(endpoint.Path, payLoad!, cancellationToken),
                "PUT" => await _client.Put<object, IResponse>(endpoint.Path, payLoad!, cancellationToken),
                "DELETE" => await _client.Get<object, IResponse>(endpoint.Path, payLoad!, cancellationToken),
                _ => throw new NotSupportedException(string.Format("Unknown HTTP method {0} for endpoint {1}", endpoint.Method, endpoint.Path))
            };
        }
    }
}
