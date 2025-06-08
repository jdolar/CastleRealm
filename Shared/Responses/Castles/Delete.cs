using Shared.Requests;
namespace Shared.Responses.Castles;
public sealed class Delete : IResponse
{
    public bool IsDeleted { get; set; }
    public Delete(bool isDeleted) => IsDeleted = isDeleted;
}