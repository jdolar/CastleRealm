using Shared.Api;
using System.Reflection;
namespace Api.Http;
public sealed class Index : IEndPoint
{
    private static DateTime started { get; } = DateTime.UtcNow;
    private static Assembly assembly = Assembly.GetExecutingAssembly();
    Domain.Info.Ident ident = new(assembly, started);
    public string Path { get; } = "/";
    public void ConfigureRoutes(IEndpointRouteBuilder app)
    { 
        app.MapGet(Path, () =>
        {
            return Results.Json(ident.Get());
        })
        .WithName(nameof(Index))
        .Produces<Shared.Responses.Info.Ident>(StatusCodes.Status200OK);
    }
}