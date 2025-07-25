﻿using Microsoft.AspNetCore.Mvc;
using Shared.Api;
namespace Api.Http;
public sealed class Tools
{
    public sealed class Encrypt : IEndPoint
    {
        public string Path { get; } = string.Format("{0}/{1}", nameof(Tools), nameof(Encrypt));
        readonly Domain.Tools.AesEncryption aes = new();
        public void ConfigureRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(Path, ([FromQuery] string input, string? aesKey, string? aesIv) =>
            {
                return Results.Ok(new Shared.Responses.Tools.Encrypt(aes.EncyrptString(input, aesKey, aesIv)));
            })
            .WithName(nameof(Encrypt))
            .WithTags(nameof(Tools))
            .Produces<Shared.Responses.Tools.Encrypt>(StatusCodes.Status200OK);
        }
    }
    public sealed class Decrypt : IEndPoint
    {
        public string Path { get; } = string.Format("{0}/{1}", nameof(Tools), nameof(Decrypt));
        readonly Domain.Tools.AesEncryption aes = new();
        public void ConfigureRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(Path, ([FromQuery] string input, string? aesKey, string? aesIv) =>
            {
                return Results.Ok(new Shared.Responses.Tools.Decrypt(aes.DecryptString(input, aesKey, aesIv)));
            })
            .WithName(nameof(Decrypt))
            .WithTags(nameof(Tools))
            .Produces<Shared.Responses.Tools.Decrypt>(StatusCodes.Status200OK);
        }
    }
}
