using Shared.Requests;
namespace Shared.Responses.Castles;
public sealed class Get : IResponse
{
    public string Name { get; set; } = string.Empty;
    public List<string>? Description { get; set; }
    public List<string>? Note { get; set; }
    public List<string>? Type { get; set; }
    public List<string>? Url { get; set; }
    public string? Country { get; set; }
    public string? Region { get; set; }
    public string? Town { get; set; }
    public string? State { get; set; }
    public string? Location { get; set; }
}
