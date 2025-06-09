using DataBase.Collections.Castles;
using Microsoft.EntityFrameworkCore;
namespace DataBase.Tools;
public sealed class Converter(CastleContext dbContext)
{
    public async Task<Shared.Responses.Castles.Get> ConvertToVisual(List<Collections.Castles.Models.Castle?> castles)
    {
        Shared.Responses.Castles.Get response = new();
        foreach (Collections.Castles.Models.Castle? castle in castles)
        {
            if (castle is null) continue;
            try
            {
                response.Castles.Add(await ConvertSingleToVisual(castle));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting castle: {ex.Message}");
            }
        }
        return response;
    }
    private async Task<Shared.Responses.Castles.GetSingle> ConvertSingleToVisual(Collections.Castles.Models.Castle? castle)
    {
        try
        {
            return new Shared.Responses.Castles.GetSingle
            {
                Id = castle?.Id,
                Added = castle!.Added,
                Updated = castle.Updated,
                Location = "1.1",// Placeholder for location

                Country = await dbContext.Country
                    .Where(c => c.Id == castle.CountryId)
                    .Select(c => c.Value)
                .FirstOrDefaultAsync(),

                Region = await dbContext.Region
                    .Where(r => r.Id == castle.RegionId)
                    .Select(r => r.Value)
                .FirstOrDefaultAsync(),

                Town = await dbContext.Town
                    .Where(s => s.Id == castle.StateId)
                    .Select(s => s.Value)
                .FirstOrDefaultAsync(),

                State = await dbContext.State
                    .Where(s => s.Id == castle.StateId)
                    .Select(s => s.Value)
                    .FirstOrDefaultAsync(),

                Type = await dbContext.Type
                    .Where(t => t.Id == castle.TypeId)
                    .Select(t => t.Value)
                    .FirstOrDefaultAsync(),

                Names = await dbContext.Name
                .Where(n => n.CastleId == castle.Id)
                .ToDictionaryAsync(n => n.Id, n => n.Value),

                Notes = await dbContext.Note
                .Where(n => n.CastleId == castle.Id)
                .ToDictionaryAsync(n => n.Id, n => n.Value),

                Descriptions = await dbContext.Description
                .Where(d => d.CastleId == castle.Id)
                .ToDictionaryAsync(d => d.Id, d => d.Value),

                Urls = await dbContext.Url
                .Where(u => u.CastleId == castle.Id)
                .ToDictionaryAsync(u => u.Id, u => u.Value),
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error converting castle: {ex.Message}");
            return new Shared.Responses.Castles.GetSingle();
        }
    }
}
