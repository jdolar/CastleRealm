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
    public static IEnumerable<object[]> EndpointData => _endpoints;
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
            IPayLoad? instance = Activator.CreateInstance(dtoType) as IPayLoad;
            object? payload = instance?.GetDefaultPayload();
            string json = JsonSerializer.Serialize(payload);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        return null;
    }

    [Fact]
    public async Task All_Endpoints_Should_Respond()
    {
        CancellationToken cancellationToken = default;
        var endpoints = await GetSwaggerEndpoints(_client);

        foreach (var (method, path, operationId) in endpoints)
        {
            HttpRequestMessage request = new(new HttpMethod(method), path);

            if (method is "POST" or "PUT")
            {
                Type? dtoType = LookupDtoForOperationId(operationId);
                HttpContent? content = TryBuildPayload(dtoType);
                if (content != null)
                {
                    request.Content = content;
                }
            }
           
            HttpResponseMessage? response = await _client.SendAsync(request, cancellationToken);
            Assert.True((int)response.StatusCode < 500, $"Failed: {method} {path} → {(int)response.StatusCode}");
        }
    }
    private Type? LookupDtoForOperationId(string operationId)
    {
        Type? interfaceType = typeof(IPayLoad);
        Assembly? assembly = interfaceType.Assembly;

        Type? handlerType = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface)
            .FirstOrDefault(t =>
                interfaceType.IsAssignableFrom(t) &&
                t.Name.Equals(operationId, StringComparison.OrdinalIgnoreCase));
        
        return handlerType;
    }
    private static async Task<IEnumerable<(string Method, string Path, string OperationId)>> GetSwaggerEndpoints(HttpClient client)
    {
        string json = await client.GetStringAsync("/swagger/v1/swagger.json");
        using JsonDocument doc = JsonDocument.Parse(json);

        JsonElement paths = doc.RootElement.GetProperty("paths");

        var endpoints = new List<(string Method, string Path, string OperationId)>();

        foreach (JsonProperty path in paths.EnumerateObject())
        {
            foreach (JsonProperty method in path.Value.EnumerateObject())
            {
                // operationId is inside each method object
                if (method.Value.TryGetProperty("operationId", out var opId))
                {
                    endpoints.Add((method.Name.ToUpperInvariant(), path.Name, opId.GetString()!));
                }
            }
        }

        return endpoints;
    }
}
