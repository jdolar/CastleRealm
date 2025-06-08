namespace Shared.Requests.Castles;
public sealed class Get : IRequest
{
    public string? Name { get; set; } = string.Empty;
    public int? Id { get; set; }
}