namespace Shared.Requests.Tools;
public sealed class Encrypt : IPayLoad
{
    public string? Input { get; set; } =  "And this run is one me!??!";
    public object GetDefaultPayload() => new Encrypt();
}