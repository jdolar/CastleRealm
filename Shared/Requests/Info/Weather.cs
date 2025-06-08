namespace Shared.Requests.Info;
public sealed class Weather : IRequest
{
    public string? City { get; set; }
}