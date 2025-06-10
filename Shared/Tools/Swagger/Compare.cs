using ApiClient;
using Microsoft.Extensions.Logging;
using System.Text;
namespace Shared.Tools.Swagger;
public sealed class Compare
{
    private readonly Health _swagger;
    private readonly IRestClient _client;
    private readonly ILogger _logger;
    public Compare(IRestClient client, ILogger logger, Requests.Tools.SwaggerCompare request)
    {
        _client = client;
        _logger = logger;
        _swagger = new(_client, _logger);
    }
    public string Three(List<Api.Endpoint> endpoints)
    {
        var grouped = endpoints
            .GroupBy(e => e.Facade)
            .ToDictionary(g => g.Key, g => g.ToList());

        return GenerateMarkdownDocumentation(grouped);
    }
    public async Task<List<Api.Endpoint>> GatherInfo(Requests.Tools.SwaggerCompare request)
    {
        List<Api.Endpoint> endpointsByFacade = new();

        foreach (string facade in request.Facades)
        {
            string facadePath = string.Format("{0}\\{1}", request.FilesPath, facade);

            bool isHealthy = await _swagger.GetStatus(facadePath);
            if (!isHealthy)
            {
                _logger.LogError("❌ Facade {0} is not healthy.", facadePath);
                continue;
            }

            List<Api.Endpoint>? facadeEndpoints = await _swagger.GetEndPoints(facadePath)!;
            if (facadeEndpoints is null) continue;

            endpointsByFacade.AddRange(facadeEndpoints);
        }
        if (endpointsByFacade.Count == 0) _logger.LogError("❌ No healthy facades found.");
        
        return endpointsByFacade;
    }
    public static string GenerateMarkdownDocumentation(Dictionary<string, List<Api.Endpoint>> groupedEndpoints)
    {
        StringBuilder sb = new();

        foreach (var group in groupedEndpoints.OrderBy(g => g.Key))
        {
            sb.AppendLine($"# {group.Key}");
            sb.AppendLine();

            foreach (var endpoint in group.Value.OrderBy(e => e.Operation))
            {
                sb.AppendLine($"## `{endpoint.Method}` {endpoint.Path}");
                sb.AppendLine($"**Operation:** `{endpoint.Operation}`");
                sb.AppendLine();

                if (endpoint.Parameters.Any())
                {
                    sb.AppendLine("| Name | In | Type | Required |");
                    sb.AppendLine("|------|----|------|----------|");

                    foreach (var p in endpoint.Parameters)
                    {
                        sb.AppendLine($"| `{p.Name}` | `{p.In}` | `{p.Type}` | `{(p.Required ? "Yes" : "No")}` |");
                    }

                    sb.AppendLine();
                }
                else
                {
                    sb.AppendLine("_No parameters._");
                    sb.AppendLine();
                }
            }

            sb.AppendLine("---");
        }

        return sb.ToString();
    }
}
