using Shared.Api;
namespace Shared.Tools.Swagger.Models;
public sealed class EndpointMatch
{
    public Endpoint? A { get; set; }
    public Endpoint? B { get; set; }
    public Endpoint? C { get; set; }

    public double? ScoreB { get; set; }
    public double? ScoreC { get; set; }
}
