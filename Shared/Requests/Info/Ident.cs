namespace Shared.Requests.Info;
public sealed class Ident : IRequest
{
    public bool? IsAdmin { get; set; } = null!;
}