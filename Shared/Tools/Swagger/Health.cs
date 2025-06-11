using ApiClient;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Shared.Api;
namespace Shared.Tools.Swagger;
public sealed class Health
{
    private readonly IRestClient _client;
    private readonly ILogger _logger;
    private readonly string _servicePath;
    public Health(IRestClient client, ILogger logger, string? servicePath = null)
    {
        _client = client;
        _logger = logger;
        _servicePath ??= servicePath ?? "/swagger/v1/swagger.json";
    }
    public async Task<bool> GetStatus(string? filePath = null)
    {
        string? json = null;
        if (string.IsNullOrWhiteSpace(filePath))
        {
            _logger.LogDebug("[GetStatus] => reading from service: {0}", _servicePath);

            try
            {
                json = await _client.Get<string>(_servicePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetStatus] => error reading file: {0}", filePath);
                return false;
            }
        }
        else
        {
            _logger.LogDebug("[GetStatus] => reading from service: {0}", filePath);

            if (!File.Exists(filePath))
            {
                _logger.LogError("[GetStatus] => file does not exist: {0}", filePath);
                return false;
            }

            try
            {
                json = await File.ReadAllTextAsync(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetStatus] => error reading file: {0}", filePath);
                return false;
            }
        }

        return CheckSwagger(json);
    }
    public async Task<List<Endpoint>?>? GetEndPoints(string? filePath = null)
    {
        string? json = null;
        _logger.LogInformation("Configuring HttpClient to retrieve Swagger endpoints...");

        if (string.IsNullOrWhiteSpace(filePath))
        {
            _logger.LogDebug("[GetEndPoints] => reading from service: {0}", _servicePath);

            try
            {
                json = await _client.Get<string>(_servicePath);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "[GetEndPoints] => Swagger not reachable: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetEndPoints] => error reading from service: {0}", _servicePath);
                return null;
            }
        }
        else
        {
            _logger.LogDebug("[GetEndPoints] => reading from file: {0}", filePath);

            if (!File.Exists(filePath))
            {
                _logger.LogError("[GetEndPoints] => file does not exist: {0}", filePath);
                return null;
            }

            try
            {
                json = await File.ReadAllTextAsync(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetEndPoints] => error reading from file: {0}", _servicePath);
                return null;
            }
        }

        return ParseSwagger(json);
    }
    public List<Endpoint>? ParseSwagger(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            _logger.LogError("Swagger JSON is empty or null.");
            return null;
        }

        try
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("paths", out JsonElement paths) ||
                paths.ValueKind != JsonValueKind.Object ||
                paths.GetRawText() == "{}")
            {
                _logger.LogError("Swagger is available but missing or empty 'paths' section.");
                return null;
            }
            if (paths.EnumerateObject().Any() == false)
            {
                _logger.LogError("Swagger 'paths' section is empty — no endpoints exposed.");
                return null;
            }

            List<Endpoint> endpoints = new();
            foreach (JsonProperty path in paths.EnumerateObject())
            {
                foreach (JsonProperty method in path.Value.EnumerateObject())
                {
                    string[] segments = path.Value.ToString().TrimStart('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
                    Endpoint endpoint = new()
                    {
                        Path = path.Name,
                        Name = Path.GetFileNameWithoutExtension(path.Name),
                        Segments = segments.Length > 0 ? segments[0] : "Unknown",
                        Method = method.Name.ToUpperInvariant()
                    };

                    // Store operationId if available
                    if (method.Value.TryGetProperty("operationId", out var operation) &&
                        operation.ValueKind == JsonValueKind.String &&
                        !string.IsNullOrWhiteSpace(operation.GetString()))
                    {
                        endpoint.Operation = operation.GetString()!;
                    }

                    // Store tags (first tag if available)
                    if (method.Value.TryGetProperty("tags", out var tags) &&
                        tags.ValueKind == JsonValueKind.Array)
                    {
                        endpoint.Tags = tags.EnumerateArray()
                                            .Select(t => t.GetString())
                                            .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));
                    }

                    // Store title if available
                    if (method.Value.TryGetProperty("title", out var title) &&
                        title.ValueKind == JsonValueKind.String &&
                        !string.IsNullOrWhiteSpace(title.GetString()))
                    {
                        endpoint.Title = title.GetString();
                    }

                    // Set MachParameter (only once) based on priority
                    if (!string.IsNullOrWhiteSpace(endpoint.Operation))
                    {
                        endpoint.MachParameter = endpoint.Operation;
                    }
                    else if (!string.IsNullOrWhiteSpace(endpoint.Tags))
                    {
                        endpoint.MachParameter = endpoint.Tags;
                    }
                    else if (!string.IsNullOrWhiteSpace(endpoint.Title))
                    {
                        endpoint.MachParameter = endpoint.Title;
                    }

                    List< Parameter> parameters = new();
                    if (method.Value.TryGetProperty("parameters", out JsonElement paramArray) &&
                        paramArray.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement param in paramArray.EnumerateArray())
                        {
                            string? name = param.GetProperty("name").GetString();
                            string? location = param.GetProperty("in").GetString();
                            bool required = param.TryGetProperty("required", out var reqProp) && reqProp.GetBoolean();

                            string type = "unknown";
                            if (param.TryGetProperty("schema", out var schema))
                            {
                                if (schema.TryGetProperty("type", out var typeProp))
                                {
                                    type = typeProp.GetString()!;
                                }
                            }

                            parameters.Add(new Parameter
                            {
                                Name = name!,
                                In = location!,
                                Required = required,
                                Type = type,
                            });
                        }
                    }

                    if (method.Value.TryGetProperty("requestBody", out JsonElement bodyArray))
                    {
                        if (bodyArray.TryGetProperty("content", out var content) &&
                            content.TryGetProperty("application/json", out var appJson) &&
                            appJson.TryGetProperty("schema", out var schema))
                        {
                            // If schema has $ref, resolve it
                            if (schema.TryGetProperty("$ref", out var refProp))
                            {
                                string refPath = refProp.GetString()!;
                                // e.g., "#/components/schemas/CreateUserRequest"
                                var schemaName = refPath.Split('/').Last();

                                if (doc.RootElement.TryGetProperty("components", out var components) &&
                                    components.TryGetProperty("schemas", out var schemas) &&
                                    schemas.TryGetProperty(schemaName, out var schemaDef))
                                {
                                    var requiredProps = new HashSet<string>();
                                    if (schemaDef.TryGetProperty("required", out var requiredArray))
                                    {
                                        requiredProps = requiredArray.EnumerateArray()
                                            .Where(x => x.ValueKind == JsonValueKind.String)
                                            .Select(x => x.GetString()!)
                                            .ToHashSet();
                                    }

                                    if (schemaDef.TryGetProperty("properties", out var properties))
                                    {
                                        foreach (var prop in properties.EnumerateObject())
                                        {
                                            string propName = prop.Name;
                                            var propSchema = prop.Value;

                                            string type = propSchema.TryGetProperty("type", out var typeProp) ? typeProp.GetString()! : "object";

                                            parameters.Add(new Parameter
                                            {
                                                Name = propName,
                                                In = "body",
                                                Required = requiredProps.Contains(propName),
                                                Type = type
                                            });
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Handle inline schema here if needed (less common)
                            }
                        }
                    }
                    
                    endpoint.Parameters = parameters;
                    endpoints.Add(endpoint);
                }
            }

            _logger.LogInformation("Discovered {0} endpoints from Swagger", endpoints.Count);
            return endpoints;
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
    public bool CheckSwagger(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            _logger.LogError("❌ Swagger JSON is empty or null");
            return false;
        }

        try
        {
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
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "❌ Invalid JSON returned from Swagger");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Unexpected error while validating Swagger");
        }
        return false;
    }
}