using DataBase.Collections.Castles.Models.Info;
using Microsoft.EntityFrameworkCore;

namespace DataBase.Collections.Castles.Data;

public sealed class Delete(CastleContext dbContext)
{
    public async Task<int?> Single(int? id, string? name)
    {
        Name? existing = await dbContext.Name.FirstOrDefaultAsync(x => x.Value == name);
        return existing?.Id;
    }
}