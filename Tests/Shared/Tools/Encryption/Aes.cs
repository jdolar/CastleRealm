using Xunit;
using Shared.Tools.Crypto;
namespace UnitTests.Shared.Tools.Encryption;
public class Aes
{
    private readonly Shared.Tools.Crypto.Aes aes = new(AesKey.Default, AesIv.Default);
    private const string encryptedValue = "pU0KDMI3g8MNKIug6EIWPGcoBC7cQmFZvsyG/KjSUQo=";
    private const string decryptedValue = "decoded aes value";

    [Fact]
    public void Encode_NotEmpty_Correct()
    {
        string encrypted = aes.Encrypt(decryptedValue);
        Assert.NotEmpty(encrypted);
        Assert.Equal(encryptedValue, encrypted);
    }

    [Fact]
    public void Decode_NotEmpty_Correct()
    {
        string decrypted = aes.Decrypt(encryptedValue);
        Assert.NotEmpty(decrypted);
        Assert.Equal(decryptedValue, decrypted);
    }

    [Fact]
    public void EncodeDecode_NotEmpty_Correct()
    {
        string randomValue = "";//Generator.NextString(Generator.NextInt(20, 100));

        string encrypted = aes.Encrypt(randomValue);
        Assert.NotEmpty(encrypted);

        string decrypted = aes.Decrypt(encrypted);
        Assert.NotEmpty(decrypted);

        Assert.Equal(randomValue, decrypted);
    }
}