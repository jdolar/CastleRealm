namespace Shared.Requests.Info;
public sealed class Ident : IPayLoad
{
    public object GetDefaultPayload() => new Ident();
}