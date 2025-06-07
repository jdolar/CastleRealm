using System.Text;
using System.Text.Json;
namespace ApiClient;
public sealed class RestClient(HttpClient httpClient) : IRestClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);
    public async Task<T?> GetAsync<T>(string uri, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(uri, cancellationToken);
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (typeof(T) == typeof(string))
                return (T)(object)content;

            return JsonSerializer.Deserialize<T>(content, _serializerOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GET] Error calling {uri}: {ex.Message}");
            return default;
        }
    }
    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string uri, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            string json = JsonSerializer.Serialize(data, _serializerOptions);
            StringContent content = new (json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(uri, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<TResponse>(responseContent, _serializerOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[POST] Error calling {uri}: {ex.Message}");
            return default;
        }
    }
    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string uri, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            string json = JsonSerializer.Serialize(data, _serializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PutAsync(uri, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<TResponse>(responseContent, _serializerOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PUT] Error calling {uri}: {ex.Message}");
            return default;
        }
    }
    public async Task<bool> DeleteAsync(string uri, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync(uri, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DELETE] Error calling {uri}: {ex.Message}");
            return false;
        }
    }
}
