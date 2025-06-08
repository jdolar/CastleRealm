using Shared.Requests;
namespace Shared.Responses.Info;
public sealed class Weather : IResponse
{
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public string? Summary { get; set; }
}