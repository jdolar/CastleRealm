namespace Shared.Requests.Castles;
public sealed class Get : IPayLoad
{
    public string? Name { get; set; } = string.Empty;
    public int? Id { get; set; }
    public object GetDefaultPayload() => new Get
    {
        Id = 1
    };
}