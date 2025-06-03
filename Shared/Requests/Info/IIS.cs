namespace Shared.Requests.Info;
public sealed class IIS : IPayLoad
{
    public object GetDefaultPayload() => new IIS();
}