using Shared.Requests;
namespace Shared.Responses.Castles;
public sealed class GetSingle : IResponse
{
    public Dictionary<int, string> Names { get; set; } = [];
    public Dictionary<int, string> Notes { get; set; } = [];
    public Dictionary<int, string> Descriptions { get; set; } = [];
    public Dictionary<int, string> Urls { get; set; } = [];
    public int? Id { get; set; }
    public string? Country { get; set; }
    public string? Region { get; set; }
    public string? Town { get; set; }
    public string? State { get; set; }
    public string? Location { get; set; }
    public string? Type { get; set; }
    public DateTime Added { get; set; }
    public DateTime Updated { get; set; }
}