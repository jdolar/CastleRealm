using Shared.Requests;
namespace Shared.Responses.Tools;
public sealed class Encrypt : IResponse
{
    public string ValueToEncrypt { get; set; }
    public Encrypt(string value) => ValueToEncrypt = value;
}