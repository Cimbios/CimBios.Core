namespace CimBios.Utils.ClassTraits.CanLog;

/// <summary>
/// Log's message interface.
/// </summary>
public interface ILogMessage
{
    public string Text { get; }
    public WeakReference SenderObject { get; }
    public LogMessageSeverity Severity { get; }
    public string CallerName { get; }
}

/// <summary>
/// Log message severity.
/// </summary>
public enum LogMessageSeverity
{
    None,
    Info,
    Warning,
    Error,
    Critical,
}

public class LogMessage(string text, LogMessageSeverity severity,
    object? senderObject = null, string callerName = "") : ILogMessage
{
    public string Text { get; } = text;
    public WeakReference SenderObject { get; } = new WeakReference(senderObject);
    public LogMessageSeverity Severity { get; } = severity;
    public string CallerName { get; } = callerName;
}

