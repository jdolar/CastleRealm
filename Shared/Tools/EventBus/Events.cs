using Microsoft.Extensions.Logging;
namespace Shared.Tools;
public sealed class Events
{
    private List<Event>? _events = null;
    public Events() => _events = new List<Event>();
    public void AddEvent(string className, string method, LogLevel loglevel, string message, object? parameters = null)
    {
        _events?.Add(new Event()
        {
            Id = _events.Count + 1,
            ClassName = className,
            Method = method,
            LogLevel = loglevel,
            Message = message,
            Parameters = parameters
        });
    }
    public void Flush() => _events?.Clear();
    public List<Event> Get() =>_events ??= new List<Event>();
}
