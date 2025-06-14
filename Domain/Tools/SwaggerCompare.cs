using ApiClient;
using Microsoft.Extensions.Logging;
using Shared.Tools.FileClient;
using Shared.Tools.Swagger;
using Shared.Tools.Swagger.Models;
using System.Diagnostics;
namespace Domain.Tools;
public class SwaggerCompare
{
    private readonly IRestClient _client;
    private readonly ILogger _logger;
    private readonly MdFile _mdFile;
    public SwaggerCompare(IRestClient client, ILogger logger)
    {
        _client = client;
        _logger = logger;
        _mdFile = new(logger);
    }
    public async Task<(string, int)> CompareAsync(Shared.Requests.Tools.SwaggerCompare request)
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();

        if (request.Facades is null || request.Facades.Count < 2 || request.Facades.Count > 3)
        {
            _logger.LogError("❌ Currently comparison is posible for only 3 swagger instances, actual number: {0}", request.Facades?.Count);
            return (string.Empty, 0);
        }

        Compare compare = new(_client, _logger, request);
        List<string> facadeLabels = request.Facades
                    .Select(f => Path.GetFileNameWithoutExtension(f))
                    .Select(name => name.Split('.').Last())
                    .ToList();

        List<List<Shared.Api.Endpoint>> swaggers = await compare.GatherInfo(request);
        if (swaggers.Count == 0)
        {
            _logger.LogError("❌ No healthy facades found.");
            return (string.Empty, 0);
        }

        List<EndpointMatch>? matches = compare.MatchEndpoints(swaggers);
        string? mdContent = _mdFile.GenerateSimplifiedMarkdown(matches, facadeLabels);

        string path = $"{request.FilesPath}\\SwaggerCompare_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.md";
        int bytesCount = await _mdFile.WriteAsync(path, mdContent);

        stopwatch.Stop();
        _logger.LogInformation("✅ Swagger comparison completed in {0} ms{1}", stopwatch.ElapsedMilliseconds, bytesCount > 0 ? ". File saved to " + path : " - nothing to write");

        return (path, bytesCount);
    } 
}