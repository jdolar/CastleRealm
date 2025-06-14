namespace Domain.Info;
public sealed class Weather
{
    private readonly string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];
    public Shared.Responses.Info.Weather Get(string? city = null)
    {
        Shared.Responses.Info.Weather[] forecast = Enumerable.Range(1, 5).Select(index =>
            new Shared.Responses.Info.Weather
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Length)]
            }).ToArray();
        return forecast[0];
    }
}
