namespace Shared.Requests.Info;
public sealed class Weather : IPayLoad
{
    public string? City { get; set; }
    public object GetDefaultPayload() => new Weather();
}