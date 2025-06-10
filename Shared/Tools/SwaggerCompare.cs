using ApiClient;
using Microsoft.Extensions.Logging;

namespace Shared.Tools;
public sealed class SwaggerCompare
{
    private const string _compareToo = "nativeapi";
    private const string _swaggerServicePath = "/swagger/v1/swagger.json";
    private const string _swaggerFilePath = "C:\\Users\\InR\\Desktop\\SwaggerCompare";
    private readonly List<string> _facades = new();
    private readonly Swagger _swagger;
    private readonly IRestClient _client;
    private readonly ILogger<Swagger> _logger;
    public SwaggerCompare(IRestClient client, ILogger<Swagger> logger)
    {
        _client = client;
        _logger = logger;
        _swagger = new(_client, _logger);
    }
}
