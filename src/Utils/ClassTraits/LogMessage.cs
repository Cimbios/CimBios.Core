namespace CimBios.Utils.ClassTraits;

/// <summary>
/// Log's message interface.
/// </summary>
public interface ILogMessage
{
    public string Title { get; }
    public string Details { get; }
    public LogMessageSeverity Severity { get; }
    public DateTime DateTimeMarker { get; }
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

public class LogMessage : ILogMessage
{
    public string Title { get; }
    public string Details { get; }
    public LogMessageSeverity Severity { get; }
    public DateTime DateTimeMarker { get; }

    public LogMessage(string title, LogMessageSeverity severity,
        string details = "")
    {
        Title = title;
        Details = details;
        Severity = severity;

        DateTimeMarker = DateTime.UtcNow;
    }
}