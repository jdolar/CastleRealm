using System.Reflection;
using System.Text;
using System.Text.Json;
using Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
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
    private readonly HttpClient _client;
    private readonly ILogger<ApiSmokeTests> _logger;
    private static readonly List<object[]> _endpoints = new();
    public ApiSmokeTests(WebApplicationFactory<Program> factory)
    {
        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development"); // Ensure Swagger is enabled
            })
            .CreateClient();

        _logger = factory.Services.GetRequiredService<ILogger<ApiSmokeTests>>();
    }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    public async ValueTask InitializeAsync()
    {
        if (_endpoints.Any())
            return;

        var swaggerEndpoints = await GetSwaggerEndpoints(_client);

        foreach (var e in swaggerEndpoints)
        {
            _endpoints.Add(new object[] { e.Method, e.Path, e.OperationId });
        }
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