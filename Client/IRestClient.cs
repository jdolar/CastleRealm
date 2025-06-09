namespace ApiClient;
public interface IRestClient
{

    Task<TResponse?> Get<TResponse>(string uri, CancellationToken cancellationToken = default);
    Task<TResponse?> Get<TRequest, TResponse>(string uri, TRequest? data, CancellationToken cancellationToken = default);
    Task<TResponse?> Post<TRequest, TResponse>(string uri, TRequest data, CancellationToken cancellationToken = default);
    Task<TResponse?> Put<TRequest, TResponse>(string uri, TRequest data, CancellationToken cancellationToken = default);
    Task<TResponse?> Delete<TRequest, TResponse>(string uri, TRequest? data, CancellationToken cancellationToken = default);
}