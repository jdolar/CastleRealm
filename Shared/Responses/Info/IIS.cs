using Microsoft.Web.Administration;
namespace Shared.Responses.Info;
public sealed class IIS : BaseResponse
{
    public string? Name { get; set; }
    public Dictionary<string, ObjectState> Pools { get; set; } = [];
    public ObjectState State { get; set; }
    public Dictionary<string, List<string>> Applications { get; set; } = [];
}
