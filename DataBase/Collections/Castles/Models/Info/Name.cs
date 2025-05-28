using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBase.Collections.Castles.Models.Info;

public sealed class Name
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int CastleId { get; set; }
    public string Value { get; set; } = string.Empty;
}
