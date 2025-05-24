using System.Text;

namespace Shared;

public static class ConsoleLogger
{
    private const string infoSeverity = "INFO";
    private const string debugSeverity = "DEBUG";
    private const string warnSeverity = "WARN";
    private const string errorSeverity = "ERROR";
    private const string fatalSeverity = "FATAL";

    private static readonly ConsoleColor infoColor = ConsoleColor.White;
    private static readonly ConsoleColor debugColor = ConsoleColor.Cyan;
    private static readonly ConsoleColor warnColor = ConsoleColor.DarkYellow;
    private static readonly ConsoleColor errorColor = ConsoleColor.Red;
    private static readonly ConsoleColor fatalColor = ConsoleColor.DarkRed;

    private static bool debugEnabled;
    private static bool timeStampEnabled;
    private static bool severityEnabled;

    private static readonly StringBuilder logMessage = new();

    #region Public Methods
    public static void Info(string message, params object[]? objects) => WriteLog(infoColor, infoSeverity, message, objects);
    public static void Debug(string message, params object[]? objects)
    {
        if (debugEnabled)
        {
            WriteLog(debugColor, debugSeverity, message, objects);
        }
    }
    public static void Warn(string message, params object[]? objects) => WriteLog(warnColor, warnSeverity, message, objects);
    public static void Error(string message, params object[]? objects) => WriteLog(errorColor, errorSeverity, message, objects);
    public static void Fatal(string message, params object[]? objects) => WriteLog(fatalColor, fatalSeverity, message, objects);
    #endregion

    #region Public Setters
    public static void DebugEnable() => debugEnabled = true;
    public static void DebugDisable() => debugEnabled = false;
    public static void LogSeverity() => severityEnabled = true;
    public static void LogTimeStamp() => timeStampEnabled = true;
    #endregion

    #region Public Getters
    public static bool DebugEnabled() => debugEnabled;
    public static bool LogSeverityEnabled() => severityEnabled;
    public static bool LogTimeStampEnabled() => timeStampEnabled;
    #endregion

    #region Private Methods
    private static void WriteLog(ConsoleColor textColor, string severity, string message, params object[]? objects)
    {
        Console.ForegroundColor = textColor;
        GetLogMessage().Clear();

        if (timeStampEnabled)
        {
            logMessage.Append(DateTime.UtcNow);
            logMessage.Append(" - ");
        }

        if (severityEnabled)
        {
            logMessage.Append('[');
            logMessage.Append(severity);
            logMessage.Append(']');
        }

        if (timeStampEnabled || severityEnabled)
        {
            logMessage.Append(' ');
        }

        if (objects?.Length > 0)
        {
            logMessage.Append(string.Format(message, objects));
        }
        else
        {
            logMessage.Append(message);
        }

        Console.WriteLine(logMessage.ToString());
    }

    private static StringBuilder GetLogMessage()
    {
        return logMessage;
    }
    #endregion
}
