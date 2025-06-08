namespace Shared.Requests.Tools;
public sealed class Decrypt : IRequest
{
    public string ValueToDecrypt { get; set; } = string.Empty;
}