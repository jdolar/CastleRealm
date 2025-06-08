namespace Shared.Requests.Castles;
public sealed class Delete : IRequest
{
    public string? Name { get; set; } = string.Empty;
    public int Id { get; set; }
}