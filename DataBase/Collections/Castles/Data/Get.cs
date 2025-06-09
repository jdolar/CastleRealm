using DataBase.Collections.Castles.Models.Info;
using Microsoft.EntityFrameworkCore;

namespace DataBase.Collections.Castles.Data;
public sealed class Get(CastleContext dbContext)
{
    public async Task<List<Models.Castle>> ByIdName(int? id, string? name)
    {
        List<Models.Castle> castles = [];

        // 1.) Search by Id
        if (id.HasValue)
        {
            Models.Castle? existing = await dbContext.Castle.FindAsync(id);
            if (existing != null) castles.Add(existing);
            return castles;
        }

        // 2.) Search by name
        if (castles.Count == 0 && !string.IsNullOrEmpty(name))
        {
            List<Name>? existingName = await dbContext.Name.Where(x => x.Value == name).ToListAsync();
            for (int i = 0; i < existingName?.Count; i++)
            {
                Models.Castle? existing = await dbContext.Castle.FindAsync(existingName[i].CastleId);
                if (existing != null) castles!.Add(existing);

            }
        }

        return castles!;
    }
}

