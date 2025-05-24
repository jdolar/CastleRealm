using DataBase.Collections.Castles;
using Shared.Tools;
using Xunit;
namespace UnitTests.DataBase;
public class ConnectionStringBuilder
{
    AesCrypto aes = new(AesKey.Default, AesIv.Default);

    [Fact]
    public void Default_NotEmpty()
    {
        string defaultConnection = ConnectionBuilder.DecryptOrGetDefault();
        Assert.NotEmpty(defaultConnection);
    }

    [Fact]
    public void SetConnectionString_NotEmpty_Correct()
    {
        string host = "localhost";
        string instance = "test";
        string database = "testDb";

        string connection = SQLServer.FormatConnectionString(host, instance, database);     
        string decrypted = ConnectionBuilder.DecryptOrGetDefault(aes.Encrypt(connection));

        Assert.NotEmpty(decrypted);
        Assert.Equal(connection, decrypted);
    }

    [Fact]
    public void RandomConnectionString_NotEmpty_Correct()
    {
        RandomGenerator gen = new();
        string host = gen.NextString(gen.NextInt(20, 100));
        string instance = gen.NextString(gen.NextInt(20, 100));
        string database = gen.NextString(gen.NextInt(20, 100));

        string randomConnection = SQLServer.FormatConnectionString(host, instance, database);
        string decrypted = ConnectionBuilder.DecryptOrGetDefault(aes.Encrypt(randomConnection));

        Assert.NotEmpty(decrypted);
        Assert.Equal(randomConnection, decrypted);
    }
}
