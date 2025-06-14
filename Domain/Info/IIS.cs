using Microsoft.Web.Administration;
namespace Domain.Info;
public sealed class IIS
{
    private readonly ServerManager? manager;
    private readonly Site? defaultSite;
    public IIS()
    {
        manager = new();
        defaultSite = manager?.Sites.First();       
    }
    public Shared.Responses.Info.IIS Get()
    {
        return new Shared.Responses.Info.IIS
        {
            State = defaultSite!.State,
            Name = defaultSite.Name,
            Pools = GetPools(),
            Applications = GetApplications()
        };
    }
    public Dictionary<string, ObjectState> GetPools()
    {
        Dictionary<string, ObjectState> pools = [];

        for (int i = 0; i < manager?.ApplicationPools.Count; i++)
        {
            pools.Add(manager.ApplicationPools[i].Name, manager.ApplicationPools[i].State);
        }

        return pools;
    }
    public Dictionary<string, List<string>> GetApplications()
    {
        Dictionary<string, List<string>> applications = [];

        for (int i = 0; i < defaultSite!.Applications.Count; i++)
        {
            List<string> vDirs = [];
            for (int j = 0; j < defaultSite.Applications[i].VirtualDirectories.Count; j++)
            {
                vDirs.Add(defaultSite.Applications[i].VirtualDirectories[j].PhysicalPath);
            }

            applications.Add(defaultSite.Applications[i].Path, vDirs);
        }

        return applications;
    }
}
