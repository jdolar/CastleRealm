using Shared.Requests;
namespace Shared.Responses.Tools;
public sealed class Encrypt : IResponse
{
    public string EncryptedValue { get; set; }
    public Encrypt(string value) => EncryptedValue = value;
}