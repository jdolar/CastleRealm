using DataBase.Collections.Castles.Models.Info;
using DataBase.Collections.Castles.Models.Static;
using Microsoft.EntityFrameworkCore;
using Type = DataBase.Collections.Castles.Models.Static.Type;

namespace DataBase.Collections.Castles;

public sealed class CastleContext(DbContextOptions<CastleContext> options) : DbContext(options)
{
    public DbSet<Models.Castle> Castle { get; set; }

    #region Info
    public DbSet<Description> Description { get; set; }
    public DbSet<Name> Name { get; set; }
    public DbSet<Note> Note { get; set; }
    public DbSet<Url> Url { get; set; }
    #endregion

    #region Static
    public DbSet<Country> Country { get; set; }
    public DbSet<Location> Location { get; set; }
    public DbSet<Region> Region { get; set; }
    public DbSet<State> State { get; set; }
    public DbSet<Town> Town { get; set; }
    public DbSet<Type> Type { get; set; }

    #endregion
}
