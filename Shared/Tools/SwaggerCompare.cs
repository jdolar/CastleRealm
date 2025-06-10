using ApiClient;
using Microsoft.Extensions.Logging;
namespace Shared.Tools;
public sealed class SwaggerCompare
{
    private readonly Swagger _swagger;
    private readonly IRestClient _client;
    private readonly ILogger _logger;
    public SwaggerCompare(IRestClient client, ILogger logger, Requests.Tools.SwaggerCompare request)
    {
        _client = client;
        _logger = logger;
        _swagger = new(_client, _logger);
    }
    public async Task<bool> CompareAsync(Requests.Tools.SwaggerCompare request)
    {
        foreach (string facade in request.Facades)
        {
            bool isHealthy = await _swagger.GetStatus(facade);
            if (!isHealthy)
            {
                _logger.LogError("❌ Facade {0} is not healthy.", facade);
                return false;
            }
        }
        return true;
    }
}
