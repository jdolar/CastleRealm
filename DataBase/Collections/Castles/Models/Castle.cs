using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBase.Collections.Castles.Models;

public sealed class Castle
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int? TypeId { get; set; }
    public int? CountryId { get; set; }
    public int? RegionId { get; set; }
    public int? TownId { get; set; }
    public int? StateId { get; set; }
    public int? LocationId { get; set; }
    public DateTime Added { get; set; }
    public DateTime Updated { get; set; }
}