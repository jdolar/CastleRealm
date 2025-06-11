using ApiClient;
using F23.StringSimilarity;
using Microsoft.Extensions.Logging;
using System.Text;
using Endpoint = Shared.Api.Endpoint;
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
    public async Task<List<List<Endpoint>>> GatherInfo(Requests.Tools.SwaggerCompare request)
    {
        List<List<Endpoint>> info = new();
        List <Api.Endpoint> endpointsByFacade = new();

        foreach (string facade in request.Facades)
        {
            string facadePath = string.Format("{0}\\{1}", request.FilesPath, facade);

            bool isHealthy = await _swagger.GetStatus(facadePath);
            if (!isHealthy)
            {
                _logger.LogError("❌ Facade {0} is not healthy.", facadePath);
                continue;
            }

            List<Endpoint>? facadeInfo = await _swagger.GetEndPoints(facadePath)!;
            if (facadeInfo is null) continue;

            info.Add(facadeInfo);
        }
        if (info.Count == 0) _logger.LogError("❌ No healthy facades found.");
        
        return info;
    }
    public List<EndpointMatch> MatchEndpoints(List<List<Endpoint>> swaggers, double threshold = 0.8)
    {
        /*
            Good enough implementation 
        */
        int swagger1 = 0;
        int swagger2 = 1;
        int swagger3 = 2;

        List<EndpointMatch> matches = new();
        HashSet<string> usedB = new();
        HashSet<string> usedC = new();
        JaroWinkler winky = new();

        foreach (Endpoint epA in swaggers[swagger1].OrderBy(x => x.Name))
        {
            Endpoint? bestB = null, bestC = null;
            double bestScoreB = 0, bestScoreC = 0;

            foreach (Endpoint epB in swaggers[swagger2])
            {
                if (usedB.Contains(epB.Name)) continue;

                double score = winky.Similarity(Normalize(epA.Name), Normalize(epB.Name));
                if (score > bestScoreB && score >= threshold)
                {
                    bestB = epB;
                    bestScoreB = score;
                }
            }

            foreach (Endpoint epC in swaggers[swagger3])
            {
                if (usedC.Contains(epC.Name)) continue;

                double score = winky.Similarity(Normalize(epA.Name), Normalize(epC.Name));
                if (score > bestScoreC && score >= threshold)
                {
                    bestC = epC;
                    bestScoreC = score;
                }
            }

            if (bestB != null) usedB.Add(bestB.Name);
            if (bestC != null) usedC.Add(bestC.Name);

            matches.Add(new EndpointMatch
            {
                A = epA,
                B = bestB,
                C = bestC,
                ScoreB = bestB != null ? bestScoreB : null,
                ScoreC = bestC != null ? bestScoreC : null
            });
        }

        // Add unmatched from B
        foreach (Endpoint sw2 in swaggers[swagger2].Where(b => !usedB.Contains(b.Name)))
        {
            matches.Add(new EndpointMatch
            {
                A = null,
                B = sw2,
                C = null,
                ScoreB = null
            });
        }

        // Add unmatched from C
        foreach (Endpoint sw3 in swaggers[swagger3].Where(c => !usedC.Contains(c.Name)))
        {
            matches.Add(new EndpointMatch
            {
                A = null,
                B = null,
                C = sw3,
                ScoreC = null
            });
        }

        return matches;
    }
    public string GenerateSimplifiedMarkdown(List<EndpointMatch> matches, List<string> facadeNames)
    {
        var sb = new StringBuilder();

        // Facade labels (one per block of 5 columns)
        sb.AppendLine($"| {facadeNames[0]} Path | Name | Method | Parameters | Misc | " +
                      $"{facadeNames[1]} Path | Name | Method | Parameters | Misc | " +
                      $"{facadeNames[2]} Path | Name | Method | Parameters | Misc |");

        // Markdown header alignment
        sb.AppendLine("|------------------|------|--------|------------|------|" +
                      "------------------|------|--------|------------|------|" +
                      "------------------|------|--------|------------|------|");

        foreach (var match in matches)
        {
            string FormatEndpoint(Endpoint? ep)
            {
                if (ep == null)
                    return "- | - | - | - | -";

                string parameters = FormatParameters(ep.Parameters).Replace("\r", "").Replace("\n", "<br>");
                string miscParts = "";

                if (!string.IsNullOrWhiteSpace(ep.Operation))
                    miscParts += $"[Operation={Escape(ep.Operation)}]";
                if (!string.IsNullOrWhiteSpace(ep.Tags))
                    miscParts += $" [Tags={Escape(ep.Tags)}]";
                if (!string.IsNullOrWhiteSpace(ep.Title))
                    miscParts += $" [Title={Escape(ep.Title)}]";

                return $"{Escape(ep.Path)} | {Escape(ep.Name)} | {Escape(ep.Method)} | {parameters} | {Escape(miscParts.Trim())}";
            }

            string row = $"| {FormatEndpoint(match.A)} | {FormatEndpoint(match.B)} | {FormatEndpoint(match.C)} |";
            sb.AppendLine(row);
        }

        return sb.ToString();
    }
    private static string DisplayName(Endpoint ep)
    {
        return Escape(ep.Tags)
            ?? Escape(ep.Title)
            ?? Escape(ep.Operation)
            ?? Escape(ep.MachParameter)
            ?? "-";
    }
    private static string Escape(string? input)
    {
        return string.IsNullOrWhiteSpace(input)
            ? "-"
            : input.Replace("|", "\\|").Replace("\n", "").Replace("\r", "").Trim();
    }
    private static string FormatEndpoint(Endpoint? ep)
    {
        if (ep == null)
            return "- | - | - | - | - | - | - | -";

        string paramText = FormatParameters(ep.Parameters).Replace("\r", "").Replace("\n", "<br>");
        string matchedOn = FormatMatchedOn(ep);

        return $"{Escape(ep.Path)} | {DisplayName(ep)} | {Escape(ep.Method)} | {paramText} | {matchedOn} | {Escape(ep.Operation)} | {Escape(ep.Tags)} | {Escape(ep.Title)}";
    }
    private static string FormatParameters(List<Parameter> parameters)
    {
        if (parameters == null || parameters.Count == 0)
            return "-";

        return string.Join("<br>", parameters.Select(p =>
            $"**{p.Name}** ({p.Type}) [{p.In}]{(p.Required ? " (required)" : "")}"
        ));
    }
    private static string FormatMatchedOn(Endpoint ep)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(ep.Operation)) parts.Add($"[Operation={Escape(ep.Operation)}]");
        if (!string.IsNullOrWhiteSpace(ep.Tags)) parts.Add($"[Tags={Escape(ep.Tags)}]");
        if (!string.IsNullOrWhiteSpace(ep.Title)) parts.Add($"[Title={Escape(ep.Title)}]");
        return parts.Count == 0 ? "-" : string.Join("", parts);
    }
    private static string Normalize(string input)
    {
        return input.Trim().ToLowerInvariant().Replace("_", "").Replace("-", "");
    }
}
