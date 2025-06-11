using ApiClient;
using Microsoft.Extensions.Logging;
using Shared.Tools.Swagger;
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
    public async Task<(string, int)> CompareAsync (Shared.Requests.Tools.SwaggerCompare request)
    {
        /*TEST ONLY*/
        request.FilesPath = "C:\\Users\\InR\\Desktop\\SwaggerCompare";
        request.Facades = new List<string> { "PlayerPortalFacade.json", "PortalGateway.json", "NativeApi.json" };

        if (request.Facades is null || request.Facades.Count == 0 || request.Facades.Count > 3)
        {
            _logger.LogError("❌ Currently comparison is posible for only 3 swagger instances, actual number: {0}", request.Facades?.Count);
            return (string.Empty, 0);
        }

        Compare compare = new(_client, _logger, request);
        List<List<Shared.Api.Endpoint>> swaggers = await compare.GatherInfo(request);
        
        List<string> facadeLabels = request.Facades
                .Select(f => Path.GetFileNameWithoutExtension(f)) 
                .Select(name => name.Split('.').Last())                   
                .ToList();

        List<EndpointMatch> maches11 = compare.MatchEndpoints(swaggers);
        string mdContent = compare.GenerateSimplifiedMarkdown(maches11, facadeLabels);

        string path = $"{request.FilesPath}\\SwaggerCompare_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.md";
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(mdContent);
        if (bytes.Length == 0)
        {
            _logger.LogError("❌ No endpoints matched.");
            return (string.Empty, 0);
        }

        await File.WriteAllBytesAsync(path, bytes);

        return (path, bytes.Length);
    }
}