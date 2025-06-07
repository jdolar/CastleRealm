namespace ApiClient;
public interface IRestClient
{
    Task<T?> GetAsync<T>(string uri, CancellationToken cancellationToken = default);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string uri, TRequest data, CancellationToken cancellationToken = default);
    Task<TResponse?> PutAsync<TRequest, TResponse>(string uri, TRequest data, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string uri, CancellationToken cancellationToken = default);
}