namespace Shared.Requests.Castles;
public sealed class Add : IPayLoad
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
    public object GetDefaultPayload() => new Add
    {
        Name = "Default Castle Name",
        Description = "Default Castle Description",
        Country = "Default Country",
        Location = "1,1",
        Note = "Default Notes",
        Region = "Default Region",
        State = "Default State",
        Town = "Default Town",
        Type = "Default Type",
        Url = "https://example.com/castle",
    };
}
