using System.Reflection;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Logging;
namespace ApiClient.Tools;
public sealed class Http
{
    private readonly Encoding _encoding = Encoding.UTF8;
    private const string _mediaType = "application/json";
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ILogger _logger;
    public Http(ILogger logger, JsonSerializerOptions? serializerOptions = null)
    {
        _logger = logger;
        _serializerOptions = serializerOptions is not null ? serializerOptions : new(JsonSerializerDefaults.Web);
    }
    public string GetUrlExtension(string baseUri, object? queryParams)
    {
        if (queryParams == null)
            return baseUri;

        try
        {
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
        catch (ArgumentException aex)
        {
            _logger.LogError(aex, "Invalid query parameters provided: {0}", aex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }

        return baseUri;
    }
    public StringContent? Serelize<TRequest>(TRequest data)
    {
        try
        {
            string json = JsonSerializer.Serialize(data, _serializerOptions);
            return new StringContent(json, _encoding, _mediaType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to serialize request data");
            return default;
        }
    }
    public async Task<TResponse?> HandleResponse<TResponse>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<TResponse>(content, _serializerOptions);
        }
        else
        {
            _logger.LogError("Failed to handle response: {0} - {1}", response.StatusCode, response.ReasonPhrase);
            return default;
        }
    }
}