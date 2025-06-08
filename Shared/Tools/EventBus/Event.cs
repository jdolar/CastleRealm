using Microsoft.Extensions.Logging;
namespace Shared.Tools;
public sealed class Event
{
    public int Id;
    public string? ClassName;
    public string? Method;
    public LogLevel? LogLevel;
    public string? Message;
    public object? Parameters;
    public override string ToString()
    {
        return string.Format("{0} [{1}] {2}.{3} | {4}{5}", Id, LogLevel, ClassName, Method, Message, Parameters);
    }
}
