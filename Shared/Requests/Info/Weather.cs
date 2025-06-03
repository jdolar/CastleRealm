namespace Shared.Requests.Info;
public sealed class Weather : IPayLoad
{
    public string? City { get; set; } = Uri.EscapeDataString("Default City");
    public object GetDefaultPayload() => new Weather();
}