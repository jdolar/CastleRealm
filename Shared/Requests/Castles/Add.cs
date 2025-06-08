namespace Shared.Requests.Castles;
public sealed class Add : IRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Note { get; set; }
    public string? Type { get; set; }
    public string? Url { get; set; }
    public string? Country { get; set; }
    public string? Region { get; set; }
    public string? Town { get; set; }
    public string? State { get; set; }
    public string? Location { get; set; }
}
