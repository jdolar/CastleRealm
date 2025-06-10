using Shared.Requests;
namespace Shared.Responses.Tools;
public sealed class Decrypt : IResponse
{
    public string ValueToDecrypt { get; set; }
    public Decrypt(string value) => ValueToDecrypt = value;
}