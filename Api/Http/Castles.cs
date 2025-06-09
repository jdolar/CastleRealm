using DataBase.Collections.Castles;
using DataBase.Tools;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
namespace Api.Http;
public sealed class Castles
{
    public sealed class Add //: IEndPoint
    {
        public string Path { get; } = string.Format("{0}/{1}", nameof(Castles), nameof(Add));
        public void ConfigureRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(Path, async (Shared.Requests.Castles.Add request, CastleContext db) =>
            {
                Domain.Castles.Castle castle = new(db);
                int castleId = await castle.Add(request);
                return Results.Ok(new Shared.Responses.Castles.Add(castleId));
            })
            .WithName(nameof(Add))
            .WithTags(nameof(Castles))
            .Produces<Shared.Responses.Castles.Add>(StatusCodes.Status200OK);

            app.MapPost(string.Format("{0}TestData", Path), async (int count, CastleContext db) =>
            {
                Domain.Castles.Castle castle = new(db);
                int castleId = await castle.AddTestData(count);
                return Results.Ok(new Shared.Responses.Castles.Add(castleId));
            })
            .WithTags(nameof(Tools))
            .Produces<Shared.Responses.Castles.Add>(StatusCodes.Status200OK);
        }
    }
    public sealed class Delete : IEndPoint
    {
        public string Path { get; } = string.Format("{0}/{1}", nameof(Castles), nameof(Delete));
        public void ConfigureRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(Path, async ([FromQuery]int? id, string? name, CastleContext db) =>
            {
                Domain.Castles.Castle castle = new(db);
                bool isDeleted = await castle.Delete(id, name);
                return Results.Ok(new Shared.Responses.Castles.Delete(isDeleted));
            })
            .WithName(nameof(Delete))
            .WithTags(nameof(Castles))
            .Produces<Shared.Responses.Castles.Delete>(StatusCodes.Status200OK);
        }
    }
    public sealed class Get : IEndPoint
    {
        public string Path { get; } = string.Format("{0}/{1}", nameof(Castles), nameof(Get));
        public void ConfigureRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(Path, async ([FromQuery] int? id, string? name, CastleContext db) =>
            {
                Converter _converter = new(db);
                Domain.Castles.Castle castle = new(db);

                List<DataBase.Collections.Castles.Models.Castle> castles = await castle.Get(id, name);
                return Results.Ok(_converter?.ConvertToVisual(castles!));
            })
            .WithName(nameof(Get))
            .WithTags(nameof(Castles))
            .Produces<List<Shared.Responses.Castles.Get>>(StatusCodes.Status200OK);
        }
    }
}
