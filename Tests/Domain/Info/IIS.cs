using Domain.Info;
using Xunit;
namespace UnitTests.Domain.Info;
public class IISInfo
{
    readonly IIS iisServer = new();
    
    // This test requires Windows OS and admin rights, hence we skip it on non-Windows platforms or when not running as admin.
    private bool AllowToRun()
    {
#if WINDOWS
        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator); 
#else
        return false;
#endif
    }

    [Fact(Skip = "Test skipped due to lack of admin rights or non-Windows OS.")]
    public void GetPools_NotEmpty()
    {
        if (!AllowToRun()) return;  // Skip the test if not allowed to run

        var pools = iisServer.GetPools();
        Assert.NotEmpty(pools);
    }

    [Fact(Skip = "Test skipped due to lack of admin rights or non-Windows OS.")]
    public void GetApplications_NotEmpty()
    {
        if (!AllowToRun()) return;  // Skip the test if not allowed to run

        var applications = iisServer.GetApplications();
        Assert.NotEmpty(applications);
    }
}
