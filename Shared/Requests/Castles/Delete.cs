namespace Shared.Requests.Castles;
public sealed class Delete : IPayLoad
{
    public string? Name { get; set; } = string.Empty;
    public int? Id { get; set; }
    public object GetDefaultPayload() => new Delete
    {
        Name = Uri.EscapeDataString("Default Castle Name"),
        Id = 1
    };
}