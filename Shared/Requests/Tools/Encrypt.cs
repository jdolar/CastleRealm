namespace Shared.Requests.Tools;
public sealed class Encrypt : IRequest
{
    public string ValueToEncrypt { get; set; } = string.Empty;
}