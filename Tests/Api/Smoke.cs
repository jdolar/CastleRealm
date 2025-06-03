using System.Reflection;
using System.Text;
using System.Text.Json;
using Api;
using DataBase.Collections.Castles;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Requests;
using Xunit;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace UnitTests.Api;
public sealed class Endpoint
{
    public string Operation { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
}

public class ApiSmokeTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private readonly ILogger<ApiSmokeTests> _logger;
    private static readonly List<object[]> _endpoints = new();
    private static List<Endpoint>? _endpointss = new();
    public ApiSmokeTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<CastleContext>>();
                services.AddDbContext<CastleContext>(options =>
                    options.UseSqlServer(SQLServer.DefaultConnection));
            });
        });
        _client = _factory.CreateClient();
        _logger = factory.Services.GetRequiredService<ILogger<ApiSmokeTests>>();
    }
    public static IEnumerable<object[]> EndpointData => _endpoints;
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    public async ValueTask InitializeAsync()
    {
        if (_endpointss!.Any())
            return;

        // Run migrations against test DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CastleContext>();
        await db.Database.MigrateAsync();

        _endpointss = await GetSwaggerEndPoints(_client)!;
    }
    private HttpContent? ConvertToHttpContent(object? payload)
    {
        string json = JsonSerializer.Serialize(payload);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }
    private object? GetDtoPayLoad(Type? dtoType)
    {
        if (dtoType == null)
            return null;

        if (typeof(IPayLoad).IsAssignableFrom(dtoType))
        {
            IPayLoad? instance = Activator.CreateInstance(dtoType) as IPayLoad;
            return instance?.GetDefaultPayload();
        }
        return null;
    }

    [Fact]
    public async Task All_Endpoints_Should_Respond()
    {
        CancellationToken cancellationToken = default;
        if (!_endpointss!.Any())
            return;

        foreach (Endpoint endpoint in _endpointss!)
        {
            HttpRequestMessage request = new(new HttpMethod(endpoint.Method), endpoint.Path);
            Type? dtoType = LookupDtoForOperationId(endpoint.Operation);

            if (endpoint.Method is "POST" or "PUT")
            {               
                HttpContent? content = ConvertToHttpContent(GetDtoPayLoad(dtoType));
                if (content != null)
                {
                    request.Content = content;
                    string json = content == null
                            ? "null"
                            : JsonSerializer.Serialize(content, new JsonSerializerOptions { WriteIndented = true });

                    var logRequest = new
                    {
                        method = endpoint.Method,
                        path = endpoint.Path,
                        operation = endpoint.Operation,
                        body = JsonSerializer.Deserialize<object>(json) // optionally deserialize if you want pretty-printed structure
                    };

                    _logger.LogInformation("REQUEST:\n{Request}", JsonSerializer.Serialize(logRequest, new JsonSerializerOptions { WriteIndented = true }));
                }
                else
                {
                    _logger.LogWarning("No payload found for operationId: {0}. Skipping request.", endpoint.Operation);
                }
            }
            else if (endpoint.Method is "GET" or "DELETE")
            {

                object? payload = GetDtoPayLoad(dtoType);
                object? payloadId = payload?.GetType().GetProperty("Id")?.GetValue(payload);
                object? payloadName = payload?.GetType().GetProperty("Name")?.GetValue(payload);
                object? payloadinput = payload?.GetType().GetProperty("Input")?.GetValue(payload);
                string urlExtension = string.Empty;

                if(urlExtension != string.Empty) urlExtension += "&";
                else urlExtension = "?";

                if (payloadId is int idValue) urlExtension += string.Format("Id={0}", idValue);

                if (urlExtension != string.Empty) urlExtension += "&";
                else urlExtension = "?";

                if (payloadName is string nameValue) urlExtension += string.Format("?Name={0}", nameValue);

                if (urlExtension != string.Empty) urlExtension += "&";
                else urlExtension = "?";
           
                if (payloadinput is string inputValue) urlExtension += string.Format("?Input={0}", inputValue);

                if (urlExtension != string.Empty)
                {
                    request.RequestUri = new Uri($"{endpoint.Path}/{urlExtension}", UriKind.Relative);
                    _logger.LogInformation("[{0}] URL override => {1}", endpoint.Method, request.RequestUri);
                }
            }

            HttpResponseMessage response = await _client.SendAsync(request, cancellationToken);
            string rawJson = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(rawJson) && rawJson.StartsWith("{"))
            {
                using JsonDocument jDoc = JsonDocument.Parse(rawJson);
                string prettyJson = JsonSerializer.Serialize(jDoc, new JsonSerializerOptions { WriteIndented = true });

                _logger.LogInformation("""
                        [{0}] {1} responded with status code {2}.
                        Response body:
                        {3}
                     """, endpoint.Method, endpoint.Path, (int)response.StatusCode, prettyJson);
            }
            else
            {
                _logger.LogInformation("[{0}] {1} responded with status code {2}. Response was not JSON: {3}",
                    endpoint.Method, endpoint.Path, (int)response.StatusCode, rawJson);
            }

            Assert.True((int)response.StatusCode < 500, $"Failed: {endpoint.Method} {endpoint.Path} → {(int)response.StatusCode}");
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
    private async Task<List<Endpoint>?>? GetSwaggerEndPoints(HttpClient client)
    {
        try
        {
            _logger.LogInformation("Configuring HttpClient to retrieve Swagger endpoints...");

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string json = await client.GetStringAsync("/swagger/v1/swagger.json");
            using JsonDocument doc = JsonDocument.Parse(json);

            JsonElement paths = doc.RootElement.GetProperty("paths");

            List<Endpoint> endpoints = new();

            foreach (JsonProperty path in paths.EnumerateObject())
            {
                foreach (JsonProperty method in path.Value.EnumerateObject())
                {
                    // operationId is inside each method object
                    if (method.Value.TryGetProperty("operationId", out var opId))
                    {
                        //endpoints.Add((method.Name.ToUpperInvariant(), path.Name, opId.GetString()!));
                        endpoints.Add(new Endpoint
                        {
                            Method = method.Name.ToUpperInvariant(),
                            Path = path.Name,
                            Operation = opId.GetString()!
                        });
                    }
                }
            }

            _logger.LogInformation("Discovered {0} endpoints from Swagger", endpoints.Count);

            return endpoints;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return null;
        }
    }
}
