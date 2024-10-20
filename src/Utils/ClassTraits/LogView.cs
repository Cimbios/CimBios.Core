using System.Collections.Concurrent;
using static CimBios.Utils.ClassTraits.ILogView;

namespace CimBios.Utils.ClassTraits;

public class PlainLogView : ILogView
{
    public IReadOnlyCollection<ILogMessage> Log { get => _Log; }
    public object Source { get; }
    public bool DebugLogMode { get; set; } = false;

    public event MessageAddedEventHandler? MessageAdded;

    public PlainLogView(object source)
    {
        Source = source;
    }

    /// <summary>
    /// Add new message method.
    /// </summary>
    /// <param name="message">ILogMessage instance.</param>
    public void NewMessage(ILogMessage message)
    {
        _Log.Add(message);

        MessageAdded?.Invoke(this, message);
    }

    /// <summary>
    /// Add new message method.
    /// </summary>
    /// <returns>New added message instance.</returns>
    public ILogMessage NewMessage(string title, 
        LogMessageSeverity severity, string details="")
    {
        var message = new LogMessage(title, 
            severity, details);

        NewMessage(message);
        return message;
    }

    protected BlockingCollection<ILogMessage> _Log 
        = new BlockingCollection<ILogMessage>();
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