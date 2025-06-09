using Shared.Requests;
namespace Shared.Responses.Castles;
public sealed class Add : IResponse
{
    public int CastleId { get; set; }
    public Add(int castleId) => CastleId = castleId;
}