using DataBase.Collections.Castles.Models;
using DataBase.Collections.Castles.Models.Info;
using Microsoft.EntityFrameworkCore;
namespace DataBase.Collections.Castles.Data;
public sealed class Delete(CastleContext dbContext)
{
    public async Task<bool> Single(int? id, string? name)
    {
        Castle? castle = null;

        // Find castle by Id
        if (id is not null) castle = await dbContext.Castle.FirstOrDefaultAsync(x => x.Id == id);

        // Find castle by Name (if by id didnt find)
        if (castle is null && name is not null)
        {
            Name? existing = await dbContext.Name.FirstOrDefaultAsync(x => x.Value == name);
            if (existing is null) return false;
            
            castle = await dbContext.Castle.FirstOrDefaultAsync(x => x.Id == existing!.CastleId);
        }

        if (castle is null) return false;

        try
        {
            dbContext.Castle.Remove(castle);
            return await dbContext.SaveChangesAsync() > 0;
        }
        catch (Exception)
        {
            return false;
        }      
    }
}