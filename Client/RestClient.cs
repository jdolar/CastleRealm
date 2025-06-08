using System.Text;
using System.Text.Json;
namespace ApiClient;
public sealed class RestClient(HttpClient httpClient) : IRestClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);
    private readonly Encoding _encoding = Encoding.UTF8;
    private const string _mediaType = "application/json";
    public async Task<T?> Get<T>(string uri, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(uri, cancellationToken);    
            
          //  if (typeof(T) == typeof(string))  return (T)(object)response;

            return await HandleResponse<T>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GET] Error calling {uri}: {ex.Message}");
            return default;
        }
    }
    public async Task<TResponse?> Get<TRequest, TResponse>(string uri, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(uri, cancellationToken);
            return await HandleResponse<TResponse>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GET] Error calling {uri}: {ex.Message}");
            return default;
        }
    }
    public async Task<TResponse?> Post<TRequest, TResponse>(string uri, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync(uri, Encode(data), cancellationToken);         
            return await HandleResponse<TResponse>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[POST] Error calling {uri}: {ex.Message}");
            return default;
        }
    }
    public async Task<TResponse?> Put<TRequest, TResponse>(string uri, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.PutAsync(uri, Encode(data), cancellationToken);
            return await HandleResponse<TResponse>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PUT] Error calling {uri}: {ex.Message}");
            return default;
        }
    }
    public async Task<TResponse?> Delete<TRequest, TResponse>(string uri, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync(uri, cancellationToken);
            return await HandleResponse<TResponse>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DELETE] Error calling {uri}: {ex.Message}");
            return default;
        }
    }
    private StringContent Encode<TRequest>(TRequest data)
    {
        string json = JsonSerializer.Serialize(data, _serializerOptions);
        return new StringContent(json, _encoding, _mediaType);
    }
    private async Task<TResponse?> HandleResponse<TResponse>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        response.EnsureSuccessStatusCode();      
        if (response.Content is null) return default;

        string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (typeof(TResponse) == typeof(string)) return (TResponse)(object)responseContent;
     
        return JsonSerializer.Deserialize<TResponse>(responseContent, _serializerOptions);
    }
}
