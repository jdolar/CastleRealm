using Shared.Requests;
namespace Api.Http;
public class Info
{
    public sealed class IIS : IRequest
    {
        public string Path { get; } = string.Format("{0}/{1}", nameof(Info), nameof(IIS));
        readonly Domain.Info.IIS serverInfo = new();
        public void ConfigureRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(Path, async () =>
            {
                return Results.Ok(serverInfo.Get());
            })
            .WithName(nameof(IIS))
            .WithTags(nameof(Info))
            .Produces<Shared.Responses.Info.IIS>(StatusCodes.Status200OK);
        }
    }
    public sealed class Weather : IRequest
    {
        public string Path { get; } = string.Format("{0}/{1}", nameof(Info), nameof(Weather));
        readonly Domain.Info.Weather weather = new();
        public void ConfigureRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(Path, async () =>
            {
                return Results.Ok(weather.Get());
            })
            .WithName(nameof(Weather))
            .WithTags(nameof(Info))
            .Produces<Shared.Responses.Info.Weather>(StatusCodes.Status200OK);
        }
    }
}