namespace UnitTests.Domain.Info;
public class IISInfo
{
    // This test requires Windows OS and admin rights, hence we skip it on non-Windows platforms or when not running as admin.
#if WINDOWS
    
    readonly IIS iisServer = new();
    private bool AllowToRun()
    {
        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
    [Fact]
    public void GetPools_NotEmpty()
    {
        if (!AllowToRun()) Assert.Skip("Not Admin");

        var pools = iisServer.GetPools();
        Assert.NotEmpty(pools);
    }

    [Fact]
    public void GetApplications_NotEmpty()
    {
        if (!AllowToRun()) Assert.Skip("Not Admin");

        var applications = iisServer.GetApplications();
        Assert.NotEmpty(applications);
    }
#endif
}
