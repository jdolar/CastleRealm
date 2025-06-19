using ApiClient;
using Microsoft.Extensions.Logging;
using Shared.Api;
using Shared.Tools.FileClient;
using Shared.Tools.Swagger.Models;
using System.IO;
using System.Text.Json;
namespace Shared.Tools.Swagger;
public sealed class Helpers
{
    private readonly IRestClient _client;
    private readonly ILogger _logger;
    private readonly string _servicePath;
    private readonly JsonFile _jsonFile;
    public Helpers(IRestClient client, ILogger logger, string? servicePath = null)
    {
        _client = client;
        _logger = logger;
        _servicePath ??= servicePath ?? "/swagger/v1/swagger.json";
        _jsonFile = new(logger);
    }
    public string? GetHighestAvailibleTier(string? tierOne, string? tierTwo, string? trierThree)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(tierOne)) return tierOne;
            else if (!string.IsNullOrWhiteSpace(tierTwo)) return tierTwo;
            else if (!string.IsNullOrWhiteSpace(trierThree)) return trierThree;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GetHighestAvailibleTier] => error determining highest tier: {0}", ex.Message);
        }

        return null;
    }
    public List<Parameter>? GetRequestBody(JsonProperty property, JsonDocument doc)
    {
        try
        {
            List<Parameter> parameters = new();

            if (property.Value.TryGetProperty("requestBody", out JsonElement bodyElement) &&
                bodyElement.TryGetProperty("content", out var contentElement) &&
                contentElement.TryGetProperty("application/json", out var appJsonElement) &&
                appJsonElement.TryGetProperty("schema", out var schemaElement))
            {
                JsonElement? refSchema = null;

                // Handle "allOf" with $ref
                if (schemaElement.TryGetProperty("allOf", out var allOfArray) &&
                    allOfArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in allOfArray.EnumerateArray())
                    {
                        if (item.TryGetProperty("$ref", out var refElement))
                        {
                            refSchema = ResolveRef(refElement.GetString()!, doc);
                            break;
                        }
                    }
                }
                // Handle direct $ref
                else if (schemaElement.TryGetProperty("$ref", out var refProp))
                {
                    refSchema = ResolveRef(refProp.GetString()!, doc);
                }

                if (refSchema.HasValue)
                {
                    JsonElement schemaDef = refSchema.Value;

                    HashSet<string> requiredProps = new();
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
                            JsonElement propSchema = prop.Value;

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
                else
                {
                    _logger.LogWarning("❗ No $ref found in schema or allOf for requestBody: {0}", schemaElement.ToString());
                }
            }

            return parameters;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GetRequestBody] => error getting request body: {0}", ex.Message);
            return null;
        }
    }
    private JsonElement? ResolveRef(string refPath, JsonDocument doc)
    {
        if (string.IsNullOrWhiteSpace(refPath))
            return null;

        string[] parts = refPath.Split('/');
        if (parts.Length < 3)
            return null;

        // Example: "#/components/schemas/SomeSchema" -> parts = [#, components, schemas, SomeSchema]
        string schemaName = parts[^1];

        if (doc.RootElement.TryGetProperty("components", out var components) &&
            components.TryGetProperty("schemas", out var schemas) &&
            schemas.TryGetProperty(schemaName, out var schemaDef))
        {
            return schemaDef;
        }

        return null;
    }
    public List<Parameter>? GetParameters(JsonProperty prop)
    {
        try
        {
            List<Parameter> parameters = new();
            if (prop.Value.TryGetProperty("parameters", out JsonElement paramArray) &&
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

                return parameters;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GetParameters] => error getting parameters property: {0}", ex.Message);
        }

        return null;
    }
    public string? GetArrayPropertyAsString(JsonProperty prop, string propName)
    {
        try
        {
            if (prop.Value.TryGetProperty("tags", out var tags) &&
            tags.ValueKind == JsonValueKind.Array)
            {
                return tags.EnumerateArray()
                                    .Select(t => t.GetString())
                                    .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GetArrayPropertyAsString] => error getting array as string property: {0}", ex.Message);
        }

        return null;
    }
    public string? GetStringProperty(JsonProperty prop, string propName)
    {
        try
        {
            if (prop.Value.TryGetProperty(propName, out var expVar) &&
                expVar.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(expVar.GetString()))
            {
                return expVar.GetString();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GetStringProperty] => error getting string property: {0}", ex.Message);
        }

        return null;
    }
    public async Task<string?> GetJson(string? filePath = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            _logger.LogDebug("[GetJson] => reading from service: {0}", _servicePath);

            try
            {
                return await _client.Get<string>(_servicePath);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "[GetJson] => Swagger not reachable: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetJson] => error reading from service: {0}", _servicePath);          
            }
        }
        else
        {
            _logger.LogDebug("[GetJson] => reading from file: {0}", filePath);

            try
            {
                return await _jsonFile.ReadAllTextAsync(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetJson] => error reading from file: {0}", _servicePath);
            }
        }

        return null;
    }
    public bool IsHealthy(string? json)
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

            if (!doc.RootElement.TryGetProperty("paths", out JsonElement paths) ||
                     paths.ValueKind != JsonValueKind.Object ||
                     paths.GetRawText() == "{}")
                {
                    _logger.LogError("❌ Missing or empty 'paths' section.");
                    return false;
                }
            if (paths.EnumerateObject().Any() == false)
            {
                _logger.LogError("❌ 'paths' section is empty — no endpoints exposed.");
                return false;
            }

            if (paths.ValueKind != JsonValueKind.Object)
            {
                _logger.LogError("❌ 'paths' is not a valid object");
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
    public static string GetNameFromPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "-";

        // Normalize and split by slash
        string[] parts = path.Trim('/').Split('/');

        if (parts.Length == 0)
            return "-";

        // Check if the last segment is a route parameter like "{id}"
        string lastSegment = parts[^1];
        bool isParam = lastSegment.StartsWith("{") && lastSegment.EndsWith("}");

        string targetSegment = isParam && parts.Length >= 2 ? parts[^2] : lastSegment;

        // Remove extension if present
        return Path.GetFileNameWithoutExtension(targetSegment);
    }
    public List<Endpoint>? GetEndPoints(string? json)
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
                    Endpoint endpoint = new()
                    {
                        Path = path.Name,
                        Name = GetNameFromPath(path.Name),
                        Method = method.Name.ToUpperInvariant(),
                        Operation = GetStringProperty(method, "operationId") ?? string.Empty,
                        Tags = GetArrayPropertyAsString(method, "tags") ?? string.Empty,
                        Title = GetStringProperty(method, "title") ?? string.Empty
                    };

                    endpoint.MachParameter = GetHighestAvailibleTier(endpoint.Operation, endpoint.Tags, endpoint.Title) ?? string.Empty;
                    endpoint.Parameters = GetParameters(method);
                    endpoint.RequestBody = GetRequestBody(method, doc);

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
}
