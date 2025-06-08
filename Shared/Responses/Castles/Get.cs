using Shared.Requests;
namespace Shared.Responses.Castles;
public sealed class Get : IResponse
{
    public List<GetSingle> Castles { get; set; } = []; 
}