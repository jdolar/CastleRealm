using ApiClient;
using F23.StringSimilarity;
using Microsoft.Extensions.Logging;
using Shared.Tools.Swagger.Models;
using Endpoint = Shared.Api.Endpoint;
namespace Shared.Tools.Swagger;
public sealed class Compare
{
    private readonly IRestClient _client;
    private readonly ILogger _logger;
    private readonly Helpers _helper;
    public Compare(IRestClient client, ILogger logger, Requests.Tools.SwaggerCompare request)
    {
        _client = client;
        _logger = logger;
        _helper = new(_client, _logger);
    }
    public async Task<List<List<Endpoint>>> GatherInfo(Requests.Tools.SwaggerCompare request)
    {
        List<List<Endpoint>> info = new();

        foreach (string facade in request.Facades)
        {
            string facadePath = string.Format("{0}\\{1}", request.FilesPath, facade);
            string? json = await _helper.GetJson(facadePath);
            bool isHealthy = _helper.IsHealthy(json);
            if (!isHealthy)
            {
                _logger.LogError("❌ Facade {0} is not healthy.", facadePath);
                continue;
            }

            List<Endpoint>? facadeInfo =  _helper.GetEndPoints(json)!;
            if (facadeInfo is null) continue;

            info.Add(facadeInfo);
        }
        
        return info;
    }
    public List<EndpointMatch>? MatchEndpoints(List<List<Endpoint>> swaggers, double threshold = 0.8)
    {
        if(swaggers.Count < 2 || swaggers.Count > 3)
        {
            _logger.LogError("❌ Currently comparison is possible for only 2 or 3 swagger instances, actual number: {0}", swaggers.Count);
            return null;
        }
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
    private string Normalize(string input)
    {
        return input.Trim().ToLowerInvariant().Replace("_", "").Replace("-", "");
    }
}
