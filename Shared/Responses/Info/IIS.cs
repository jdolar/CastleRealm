using Microsoft.Web.Administration;
using Shared.Requests;
namespace Shared.Responses.Info;
public sealed class IIS : IResponse
{
    public string? Name { get; set; }
    public Dictionary<string, ObjectState> Pools { get; set; } = [];
    public ObjectState State { get; set; }
    public Dictionary<string, List<string>> Applications { get; set; } = [];
}
