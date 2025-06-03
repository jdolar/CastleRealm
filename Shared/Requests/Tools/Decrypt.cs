namespace Shared.Requests.Tools;
public sealed class Decrypt : IPayLoad
{
    public string? Input { get; set; } =  Uri.EscapeDataString("7pKbTM5o/p+6I4mIpkEpi3pUlCxxp546iFbcb5Ke/5s=");
    public object GetDefaultPayload() => new Decrypt();
}