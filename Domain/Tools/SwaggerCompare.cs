using ApiClient;
using Microsoft.Extensions.Logging;
namespace Domain.Tools;
public class SwaggerCompare
{
    private readonly IRestClient _client;
    private readonly ILogger _logger;
    public SwaggerCompare(IRestClient client, ILogger logger)
    {
        _client = client;
        _logger = logger;     
    }
    public async Task<bool> CompareAsync (Shared.Requests.Tools.SwaggerCompare request)
    {
        if (request.Facades is null || request.Facades.Count == 0)
        {
            _logger.LogError("❌ No facades provided for comparison.");
            return false;
        }

        Shared.Tools.SwaggerCompare sw = new(_client, _logger, request);
        return await sw.CompareAsync(request);
    }
}