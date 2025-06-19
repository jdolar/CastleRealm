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
using Shared.Api;
using Shared.Tools.Swagger;
using ApiClient.Tools;
using Microsoft.AspNetCore.Hosting;
using Shared.Tools.SimpleLogger;
using Microsoft.Extensions.Configuration;
using System.Reflection;
namespace UnitTests.Api;
public class ApiSmokeTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    //private readonly ITestOutputHelper _output;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly IRestClient _client;
    private readonly Helpers _swagger;
    private readonly Content _content;
    private readonly ILogger _logger;
    private static List<Endpoint>? _endpoints = new();
    private readonly Http _utils;
    public ApiSmokeTests(WebApplicationFactory<Program> factory)
    {
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
            //builder.ConfigureLogging((context, loggingBuilder) =>
            //{
            //    loggingBuilder.ClearProviders();
            //    IConfigurationRoot config = Configure.BuildConfiguration();
            //    Configure.ConfigureLogging(loggingBuilder, config);
            //    loggingBuilder.AddConsole();
            //});
            //builder.ConfigureLogging((context, loggingBuilder) =>
            //{
            //    IConfigurationRoot config = Configure.BuildConfiguration();
            //    Configure.ConfigureLogging(loggingBuilder, config);
            //    loggingBuilder.AddConsole();
            //});
        });

        _logger = _factory.Services.GetRequiredService<ILogger<ApiSmokeTests>>();
        _client = _factory.Services.GetRequiredService<IRestClient>();
        _utils = new(_logger);
        _swagger = new(_client, _logger);
        _content = new(_logger);

        var a = _logger.GetType();

        var loggerFactory = _factory.Services.GetRequiredService<ILogger<ApiSmokeTests>>();
        var providersField = loggerFactory.GetType().GetField("_providers", BindingFlags.NonPublic | BindingFlags.Instance);
        var providers = providersField?.GetValue(loggerFactory) as IEnumerable<ILoggerProvider>;

        if (providers != null)
        {
            foreach (var provider in providers)
            {
                Console.WriteLine($"[DEBUG] Provider: {provider.GetType().FullName}");
            }
        }
        else
        {
            Console.WriteLine("⚠️ Could not reflect logger providers.");
        }
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

        string? json = await _swagger.GetJson();
        bool isHealthy = _swagger.IsHealthy(json);
        if (isHealthy) _endpoints = _swagger.GetEndPoints(json)!;
    }

    [Fact]
    public async Task ApiSmoke_AllEndpoints_Success()
    {
        //_output.WriteLine("This appears in test output window");
        CancellationToken cancellationToken = default;
        if (!_endpoints!.Any())
            return;

        foreach (Endpoint endpoint in _endpoints!)
        {
            Type? type = _content.GetDtoType(endpoint.Name);
            if (type is null) continue;

            object? payLoad = _content.GetPayLoad(type!);
            IResponse? result = endpoint.Method.ToUpperInvariant() switch
            {
                "GET" => await _client.Get<object, IResponse>(_utils.GetUrlExtension(endpoint.Path, payLoad), null, cancellationToken),
                "POST" => await _client.Post<object, IResponse>(endpoint.Path, payLoad!, cancellationToken),
                "PUT" => await _client.Put<object, IResponse>(endpoint.Path, payLoad!, cancellationToken),
                "DELETE" => await _client.Delete<object, IResponse>(_utils.GetUrlExtension(endpoint.Path, payLoad), null, cancellationToken),
                _ => throw new NotSupportedException(string.Format("Unknown HTTP method {0} for endpoint {1}", endpoint.Method, endpoint.Path))
            };
        }
    }
}
