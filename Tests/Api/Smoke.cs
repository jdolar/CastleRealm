using System.Text.Json;
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
using System.Drawing;
using Shared.Api;
namespace UnitTests.Api;
public class ApiSmokeTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private IRestClient _client;
    private readonly Swagger _swagger;
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
    private static HttpContent? TryBuildPayload(Type? dtoType)
    {
        if (dtoType == null)
            return null;

        if (typeof(IPayLoad).IsAssignableFrom(dtoType))
        {
            string method = endpoint.Method.ToUpperInvariant();

            Type? type = _swagger.GetDtoType(endpoint.Operation);
            if (type is null) continue;

            object? payLoad = _swagger.GetPayLoad(type!);
            if (payLoad is null) continue;

            string uri = method switch
            {
                "DELETE" or "GET" => _swagger.GetUrlExtension(endpoint.Path, payLoad),
                _ => endpoint.Path
            };

            try
            {
                IResponse? result = method switch
                {
                    "GET" => await _client.Get<object, IResponse>(uri, payLoad!, cancellationToken),
                    "POST" => await _client.Post<object, IResponse>(uri, payLoad!, cancellationToken),
                    "PUT" => await _client.Put<object, IResponse>(uri, payLoad!, cancellationToken),
                    "DELETE" => await _client.Get<object, IResponse>(uri, payLoad!, cancellationToken),
                    _ => throw new NotSupportedException($"Unsupported HTTP method: {method}")
                };

                var logRequest = new
                {
                    method = endpoint.Method,
                    path = uri,
                    operation = endpoint.Operation,
                    body = method switch
                    {
                        "POST" or "PUT" => JsonSerializer.Deserialize<object>((JsonDocument)payLoad!),
                        _ => string.Empty
                    }
                };

                _logger.LogInformation("REQUEST:\n{Request}", JsonSerializer.Serialize(logRequest, new JsonSerializerOptions { WriteIndented = true }));

            }
            catch (Exception ex)
            {
                var logRequest = new
                {
                    method = endpoint.Method,
                    path = uri,
                    operation = endpoint.Operation,
                    body = method switch
                    {
                        "POST" or "PUT" => JsonSerializer.Deserialize<object>((JsonDocument)payLoad!),
                        _ => string.Empty
                    }
                };

                _logger.LogError(ex, "❌ Error calling {0} {1}\n{2}",endpoint.Method, uri, JsonSerializer.Serialize(logRequest, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
    }
}
