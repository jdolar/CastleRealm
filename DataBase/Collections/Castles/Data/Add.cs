using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Type = DataBase.Collections.Castles.Models.Static.Type;
using DataBase.SQL;
using DataBase.Collections.Castles.Models.Info;
using DataBase.Collections.Castles.Models.Static;
namespace DataBase.Collections.Castles.Data;
public sealed class Add(CastleContext dbContext)
{
    public async Task<int?> Single(Shared.Requests.Castles.Add request)
    {       
        Name? existing = await dbContext.Name.FirstOrDefaultAsync(x => x.Value == request.Name);
        if (existing != null)
        {
            return existing.Id;
        }
        else
        {
            int? castleId = null;
            int? countryId = await ExecuteCustomMergeAsync(dbContext.Database, nameof(Country), request.Country!);
            int? regionId = await ExecuteCustomMergeAsync(dbContext.Database, nameof(Region), request.Region!);
            int? stateId = await ExecuteCustomMergeAsync(dbContext.Database, nameof(State), request.State!);
            int? townId = await ExecuteCustomMergeAsync(dbContext.Database, nameof(Town), request.Town!);
            int? typeId = await ExecuteCustomMergeAsync(dbContext.Database, nameof(Type), request.Type!);

            // Location
            int? locationId = null;
            string[]? locationParsed = request.Location?.Split(',');
            if (locationParsed?.Length > 0)
            {
                EntityEntry<Location> loc = await dbContext.Location.AddAsync(new Location()
                {
                    X = Convert.ToInt32(locationParsed[0]),
                    Y = Convert.ToInt32(locationParsed[1]),
                });
                locationId = loc.Entity.Id;
            }

            // Save all entities at once
            await dbContext.SaveChangesAsync();

            // Create Castle
            EntityEntry<Models.Castle> castleEntity = await dbContext.Castle.AddAsync(new Models.Castle()
            {
                Added = DateTime.UtcNow,
                CountryId = countryId,
                LocationId = locationId,
                RegionId = regionId,
                StateId = stateId,
                TownId = townId,
                TypeId = typeId
            });

            // Save Castle
            await dbContext.SaveChangesAsync();
            castleId = castleEntity.Entity.Id;

            await dbContext.Description.AddAsync(new Description()
            {
                CastleId = (int)castleId,
                Value = request.Description!
            });

            await dbContext.Name.AddAsync(new Name()
            {
                CastleId = (int)castleId,
                Value = request.Name!
            });

            await dbContext.Note.AddAsync(new Note()
            {
                CastleId = (int)castleId,
                Value = request.Note!
            });

            await dbContext.Url.AddAsync(new Url()
            {
                CastleId = (int)castleId,
                Value = request.Url!
            });

            // Save the final related entities
            await dbContext.SaveChangesAsync();

            return castleId;
        }
    }
    private static async Task<int?> ExecuteCustomMergeAsync(DatabaseFacade db, string tableName, string fieldValue, string? fieldName = "Value")
    {
        try
        {
            return (await db
                .SqlQueryRaw<int>(
                    Merge.FormatSql(tableName, fieldName!),
                    new SqlParameter(string.Join("@", fieldName), fieldValue)
                )
                .ToListAsync())
                .FirstOrDefault();
        }
        catch (Exception)
        {
            return null;
        }
    }
}