using Shared.Requests;
using System.Reflection;
namespace Api.Http;
public class Info
{
    public sealed class IIS : IRequest
    {
        public string Path { get; } = string.Format("{0}/{1}", nameof(Info), nameof(IIS));
        readonly Domain.Info.IIS serverInfo = new();
        public void ConfigureRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(Path, () =>
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
            app.MapGet(Path, () =>
            {
                return Results.Ok(weather.Get());
            })
            .WithName(nameof(Weather))
            .WithTags(nameof(Info))
            .Produces<Shared.Responses.Info.Weather>(StatusCodes.Status200OK);
        }
    }
    public sealed class Ident : IRequest
    {
        public string Path { get; } = string.Format("{0}/{1}", nameof(Info), nameof(Ident));
        private static DateTime started { get; } = DateTime.UtcNow;
        private static Assembly assembly = Assembly.GetExecutingAssembly();
        readonly Domain.Info.Ident info = new(assembly, started);
        public void ConfigureRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(Path, () =>
            {
                return Results.Ok(info.Get());
            })
            .WithName(nameof(Ident))
            .WithTags(nameof(Info))
            .Produces<Shared.Responses.Info.Ident>(StatusCodes.Status200OK);
        }
    }
}