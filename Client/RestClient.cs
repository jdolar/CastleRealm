using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
namespace ApiClient;
public sealed class RestClient(HttpClient httpClient, ILogger<RestClient> logger) : IRestClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<RestClient> _logger = logger;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);
    private readonly Encoding _encoding = Encoding.UTF8;
    private const string _mediaType = "application/json";
    public async Task<T?> Get<T>(string uri, CancellationToken cancellationToken = default)
    {
        return await Get<T, T>(uri, default, cancellationToken);
    }
    public async Task<TResponse?> Get<TRequest, TResponse>(string uri, TRequest? data, CancellationToken cancellationToken = default)
    {
        try
        {
            if (data is not null) uri = Helper.GetUrlExtension(uri, data);

            HttpResponseMessage response = await _httpClient.GetAsync(uri, cancellationToken);
            return await HandleResponse<TResponse>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GET] Error calling {0}: {1}", uri, ex.Message);
            return default;
        }
    }
    public async Task<TResponse?> Post<TRequest, TResponse>(string uri, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync(uri, Serelize(data), cancellationToken);         
            return await HandleResponse<TResponse>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[POST] Error calling {0}: {1}", uri, ex.Message);
            return default;
        }
    }
    public async Task<TResponse?> Put<TRequest, TResponse>(string uri, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.PutAsync(uri, Serelize(data), cancellationToken);
            return await HandleResponse<TResponse>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PUT] Error calling {0}: {1}", uri, ex.Message);
            return default;
        }
    }
    public async Task<TResponse?> Delete<TRequest, TResponse>(string uri, TRequest? data, CancellationToken cancellationToken = default)
    {
        try
        {
            if (data is not null) uri = Helper.GetUrlExtension(uri, data);

            HttpResponseMessage response = await _httpClient.DeleteAsync(uri, cancellationToken);
            return await HandleResponse<TResponse>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DELETE] Error calling {0}: {1}", uri,ex.Message);
            return default;
        }
    }
    private async Task<TResponse?> HandleResponse<TResponse>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        response.EnsureSuccessStatusCode();      
        if (response.Content is null) return default;

        string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (typeof(TResponse) == typeof(string)) return (TResponse)(object)responseContent;
     
        return JsonSerializer.Deserialize<TResponse>(responseContent, _serializerOptions);
    }
    private StringContent? Serelize<TRequest>(TRequest data)
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
}