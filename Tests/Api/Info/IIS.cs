
using Domain.Info;
using Xunit;
namespace UnitTests.Api.Info;

public class IISInfo
{
    readonly IIS iisServer = new();

    [Fact]
    public void GetPools_NotEmpty()
    {
        var pools = iisServer.GetPools();
        Assert.NotEmpty(pools);
    }

    [Fact]
    public void GetApplications_NotEmpty()
    {
        var applications = iisServer.GetApplications();
        Assert.NotEmpty(applications);
    }
}
