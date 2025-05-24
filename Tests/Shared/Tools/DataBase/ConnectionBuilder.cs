using Shared.Tools.Database;
using Shared.Tools.Random;
using Xunit;

namespace UnitTests.Shared.Tools.DataBase;

public class ConnectionStringBuilder
{
    [Fact]
    public void Default_NotEmpty()
    {
        string defaultConnection = ConnectionBuilder.DecryptOrGetDefault();
        Assert.NotEmpty(defaultConnection);
    }

    [Fact]
    public void NotEmpty()
    {
        string host = "localhost";
        string instance = "test";
        string database = "testDb";

        string randomConnection = SQLServer.FormatConnectionString(host, instance, database);
        string decrypted = ConnectionBuilder.DecryptOrGetDefault(randomConnection);

        Assert.NotEmpty(decrypted);
    }

    [Fact]
    public void Random_NotEmpty()
    {
        Generator gen = new();
        string randomHost = gen.NextString(gen.NextInt(20, 100));
        string randomInstance = gen.NextString(gen.NextInt(20, 100));
        string randomDataBase = gen.NextString(gen.NextInt(20, 100));

        string randomConnection = SQLServer.FormatConnectionString(randomHost, randomInstance, randomDataBase);
        string decrypted = ConnectionBuilder.DecryptOrGetDefault(randomConnection);

        Assert.NotEmpty(decrypted);
    }
}
