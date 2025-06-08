using ApiClient;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Shared.Requests;
using System.Reflection;
using Shared.Api;

namespace Shared.Tools;
public sealed class Swagger
{
    private readonly IRestClient _client;
    private readonly ILogger _logger;
    private const string _swaggerPath = "/swagger/v1/swagger.json";
    private readonly RandomGenerator _randomGenerator = new();
    public Swagger(IRestClient client, ILogger logger)
    {
        _client = client;
        _logger = logger;
    }
    public async Task<bool> GetStatus()
    {
        try
        {
            string? json = await _client.Get<string>(_swaggerPath);
            if (string.IsNullOrWhiteSpace(json))
            {
                _logger.LogError("❌ Swagger JSON is empty or null");
                return false;
            }

            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            if (!root.TryGetProperty("openapi", out var openapi))
            {
                _logger.LogError("❌ Missing 'openapi' property");
                return false;
            }

            if (!root.TryGetProperty("info", out var info) ||
                !info.TryGetProperty("title", out _) ||
                !info.TryGetProperty("version", out _))
            {
                _logger.LogError("❌ Missing 'info.title' or 'info.version'");
                return false;
            }

            if (!root.TryGetProperty("paths", out var paths))
            {
                _logger.LogError("❌ Missing 'paths' section");
                return false;
            }

            if (paths.ValueKind != JsonValueKind.Object)
            {
                _logger.LogError("❌ 'paths' is not a valid object");
                return false;
            }

            if (!paths.EnumerateObject().Any())
            {
                _logger.LogError("❌ Swagger 'paths' is empty — no endpoints exposed");
                return false;
            }

            _logger.LogDebug("✅ Swagger is healthy:");
            _logger.LogDebug("  - OpenAPI: {OpenApi}", openapi.GetString());
            _logger.LogDebug("  - Endpoints: {EndpointCount}", paths.EnumerateObject().Count());
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "❌ HTTP error while fetching Swagger");
            return false;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "❌ Invalid JSON returned from Swagger");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Unexpected error while validating Swagger");
            return false;
        }
    }
    public async Task<List<Endpoint>?>? GetEndPoints()
    {
        try
        {
            _logger.LogInformation("Configuring HttpClient to retrieve Swagger endpoints...");

            string? json = await _client.Get<string>("/swagger/v1/swagger.json");
            if (string.IsNullOrWhiteSpace(json))
            {
                _logger.LogError("Swagger JSON is empty or null.");
                return null;
            }

            using JsonDocument doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("paths", out JsonElement paths) ||
                paths.ValueKind != JsonValueKind.Object ||
                paths.GetRawText() == "{}")
            {
                _logger.LogError("Swagger is available but missing or empty 'paths' section.");
                return null;
            }

            List<Endpoint> endpoints = new();
            foreach (JsonProperty path in paths.EnumerateObject())
            {
                foreach (JsonProperty method in path.Value.EnumerateObject())
                {
                    if (method.Value.TryGetProperty("operationId", out var opId))
                    {
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
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Swagger not reachable: {0}", ex.Message);
        }
        catch (InvalidDataException ex)
        {
            _logger.LogError(ex, "Swagger is incomplete or malformed: {0}", ex.Message);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON from Swagger: {0}", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving Swagger endpoints: {0}", ex.Message);
        }

        return null;
    }
    public Type? GetDtoType(string operationId)
    {
        Type? interfaceType = typeof(IRequest);
        Assembly? assembly = interfaceType.Assembly;

        Type? handlerType = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface)
            .FirstOrDefault(t =>
                interfaceType.IsAssignableFrom(t) &&
                t.Name.Equals(operationId, StringComparison.OrdinalIgnoreCase));

        return handlerType;
    }
    public object GetPayLoad(Type type)
    {
        if (type == typeof(string)) return _randomGenerator.NextString(5);
        if (type == typeof(int)) return 1;
        if (type == typeof(DateTime)) return DateTime.UtcNow;

        object? instance = Activator.CreateInstance(type)!;
        foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanWrite) continue;

            object? value = GetDefaultValue(prop.PropertyType);
            prop.SetValue(instance, value);
        }

        return instance;
    }
    public string GetUrlExtension(string baseUri, object queryParams)
    {
        if (queryParams == null)
            return baseUri;

        PropertyInfo[] props = queryParams.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        string query = string.Join("&", props
            .Where(p => p.GetValue(queryParams) != null)
            .Select(p =>
                $"{Uri.EscapeDataString(p.Name)}={Uri.EscapeDataString(p.GetValue(queryParams)!.ToString()!)}"
            )
        );

        return string.IsNullOrWhiteSpace(query)
            ? baseUri
            : $"{baseUri}?{query}";
    }
    private object? GetDefaultValue(Type type)
    {
        if (type == typeof(string)) return _randomGenerator.NextString(10);
        if (type == typeof(int)) return 1;
        if (type == typeof(DateTime)) return DateTime.UtcNow;
        if (type == typeof(bool)) return true;
        if (type.IsEnum) return Enum.GetValues(type).GetValue(0);
        if (type.IsClass && type != typeof(string)) return GetPayLoad(type);
        if (Nullable.GetUnderlyingType(type) is { } innerType)
            return GetDefaultValue(innerType);

        return null;
    }
}