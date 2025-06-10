using Shared.Requests;
namespace Shared.Responses.Tools;
public sealed class SwaggerCompared : IResponse
{
    public bool IsDone { get; set; } = new();
    public SwaggerCompared(bool isDone) => IsDone = isDone;
}
