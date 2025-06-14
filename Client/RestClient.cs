using ApiClient.Tools;
using Microsoft.Extensions.Logging;
namespace ApiClient;
public sealed class RestClient(HttpClient httpClient, ILogger<RestClient> logger) : IRestClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<RestClient> _logger = logger;
    private Http _utils = new(logger);
    public async Task<T?> Get<T>(string uri, CancellationToken cancellationToken = default)
    {
        return await Get<T, T>(uri, default, cancellationToken);
    }
    public async Task<TResponse?> Get<TRequest, TResponse>(string uri, TRequest? data, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(uri, cancellationToken);
            return await _utils.HandleResponse<TResponse>(response, cancellationToken);
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
            HttpResponseMessage response = await _httpClient.PostAsync(uri, _utils.Serelize(data), cancellationToken);         
            return await _utils.HandleResponse<TResponse>(response, cancellationToken);
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
            HttpResponseMessage response = await _httpClient.PutAsync(uri, _utils.Serelize(data), cancellationToken);
            return await _utils.HandleResponse<TResponse>(response, cancellationToken);
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
            HttpResponseMessage response = await _httpClient.DeleteAsync(uri, cancellationToken);
            return await _utils.HandleResponse<TResponse>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DELETE] Error calling {0}: {1}", uri,ex.Message);
            return default;
        }
    }
}