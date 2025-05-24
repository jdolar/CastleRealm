namespace Shared.Responses.Info;
public sealed class Weather : BaseResponse
{
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public string? Summary { get; set; }
}