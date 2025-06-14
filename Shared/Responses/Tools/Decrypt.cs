using Shared.Requests;
namespace Shared.Responses.Tools;
public sealed class Decrypt : IResponse
{
    public string DecryptedValue { get; set; }
    public Decrypt(string value) => DecryptedValue = value;
}