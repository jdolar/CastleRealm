using DataBase.Collections.Castles;
using Shared.Requests;
namespace Api.Http;
public sealed class Castles
{
    public sealed class Add : IRequest
    {
        public string Path { get; } = string.Format("{0}/{1}", nameof(Castles), nameof(Add));
        public void ConfigureRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(Path, async (Shared.Requests.Castles.Add request, CastleContext db) =>
            {
                Domain.Castles.Castle castle = new(db);
                int? castleId = await castle.Add(request);
                return Results.Ok(castleId);
            })           
            .WithName(nameof(Add))
            .WithTags(nameof(Castles))
            .Produces<Shared.Responses.BaseResponse>(StatusCodes.Status200OK);

            app.MapPost(string.Format("{0}TestData", Path), async (int count, CastleContext db) =>
            {
                Domain.Castles.Castle castle = new(db);
                int? castleId = await castle.AddTestData(count);
                return Results.Ok(castleId);
            })
            .WithTags(nameof(Tools))
            .Produces<Shared.Responses.BaseResponse>(StatusCodes.Status200OK);
        }
    }
    public sealed class Delete : IRequest
    {
        public string Path { get; } = string.Format("{0}/{1}", nameof(Castles), nameof(Delete));
        public void ConfigureRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(Path, async (int id, CastleContext db) =>
            {
                Domain.Castles.Castle castle = new(db);
                await castle.Delete(id);
                return Results.Ok;
            })
            .WithName(nameof(Delete))
            .WithTags(nameof(Castles))
            .Produces<Shared.Responses.BaseResponse>(StatusCodes.Status200OK);
        }
    }
    public sealed class Get : IRequest
    {
        public string Path { get; } = string.Format("{0}/{1}", nameof(Castles), nameof(Get));
        public void ConfigureRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(Path, async (int? id, string? name, CastleContext db) =>
            {
                Domain.Castles.Castle castle = new(db);
                var response = await castle.Get(id, name);
                return Results.Ok(response);
            })
            .WithName(nameof(Get))
            .WithTags(nameof(Castles))
            .Produces<Shared.Responses.BaseResponse>(StatusCodes.Status200OK);
        }
    }
}
