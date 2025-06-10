using Shared.Requests;
namespace Shared.Responses.Castles;
public sealed class Added : IResponse
{
    public int CastleId { get; set; }
    public Added(int castleId) => CastleId = castleId;
}