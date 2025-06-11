using ApiClient;
using Microsoft.Extensions.Logging;
using Shared.Tools.Swagger;
using System.Diagnostics;
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
        Stopwatch stopwatch = new();
        if (request.Facades is null || request.Facades.Count < 2 || request.Facades.Count > 3)
        {
            _logger.LogError("❌ Currently comparison is posible for only 3 swagger instances, actual number: {0}", request.Facades?.Count);
            return (string.Empty, 0);
        }

        stopwatch.Start();
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
        stopwatch.Stop();
        _logger.LogInformation("✅ Swagger comparison completed in {0} ms. File saved to {1}", stopwatch.ElapsedMilliseconds, path);
        return (path, bytes.Length);
    }
}