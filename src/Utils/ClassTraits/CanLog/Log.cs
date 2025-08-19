using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using static CimBios.Utils.ClassTraits.CanLog.ILogView;

namespace CimBios.Utils.ClassTraits.CanLog;

/// <summary>
///     Log messages interface. Provides log messages logging access.
/// </summary>
public interface ILog : ILogView
{
    /// <summary>
    ///     Set debug mode - log debug messages.
    /// </summary>
    public bool DebugLogMode { get; set; }

    /// <summary>
    ///     Add new message method.
    /// </summary>
    /// <param name="message">ILogMessage instance.</param>
    public void NewMessage(ILogMessage message);

    /// <summary>
    ///     Add new message method.
    /// </summary>
    /// <returns>New added message instance.</returns>
    public ILogMessage NewMessage(string text, LogMessageSeverity severity,
        object? senderObject = null, [CallerMemberName] string callerName = "");

    /// <summary>
    ///     Add new info message method.
    /// </summary>
    /// <returns>New added message instance.</returns>
    public ILogMessage Info(string text, object? senderObject = null,
        [CallerMemberName] string callerName = "");

    /// <summary>
    ///     Add new warning message method.
    /// </summary>
    /// <returns>New added message instance.</returns>
    public ILogMessage Warn(string text, object? senderObject = null,
        [CallerMemberName] string callerName = "");

    /// <summary>
    ///     Add new error message method.
    /// </summary>
    /// <returns>New added message instance.</returns>
    public ILogMessage Error(string text, object? senderObject = null,
        [CallerMemberName] string callerName = "");

    /// <summary>
    ///     Add new critical message method.
    /// </summary>
    /// <returns>New added message instance.</returns>
    public ILogMessage Critical(string text, object? senderObject = null,
        [CallerMemberName] string callerName = "");

    /// <summary>
    ///     Return read-only log view wrapper.
    /// </summary>
    /// <returns>Read only wrapper.</returns>
    public ILogView AsReadOnly();

    /// <summary>
    /// </summary>
    public void Clear();

    /// <summary>
    /// </summary>
    public void FlushFrom(ILogView logView, bool silent = false);
}

/// <summary>
///     Simple plain log. Provides logs set and events logic.
/// </summary>
public class PlainLogView : ILog
{
    protected BlockingCollection<ILogMessage> _Log = [];

    public PlainLogView(object source)
    {
        Source = source;

        if (DebugLogMode)
            NewMessage(
                $"{source.GetType().FullName}: Log view initialized",
                LogMessageSeverity.Info
            );
    }

    public IReadOnlyCollection<ILogMessage> Messages => _Log;
    public object Source { get; }
    public bool DebugLogMode { get; set; } = false;

    public event MessageAddedEventHandler? MessageAdded;

    public void NewMessage(ILogMessage message)
    {
        _Log.Add(message);

        MessageAdded?.Invoke(this, message);
    }

    public ILogMessage NewMessage(string text, LogMessageSeverity severity,
        object? senderObject = null, [CallerMemberName] string callerName = "")
    {
        var message = new LogMessage(text, severity, senderObject, callerName);

        NewMessage(message);
        return message;
    }

    public ILogMessage Info(string text, object? senderObject = null,
        [CallerMemberName] string callerName = "")
    {
        return NewMessage(text, LogMessageSeverity.Info,
            senderObject, callerName);
    }

    public ILogMessage Warn(string text, object? senderObject = null,
        [CallerMemberName] string callerName = "")
    {
        return NewMessage(text, LogMessageSeverity.Warning,
            senderObject, callerName);
    }

    public ILogMessage Error(string text, object? senderObject = null,
        [CallerMemberName] string callerName = "")
    {
        return NewMessage(text, LogMessageSeverity.Error,
            senderObject, callerName);
    }

    public ILogMessage Critical(string text, object? senderObject = null,
        [CallerMemberName] string callerName = "")
    {
        return NewMessage(text, LogMessageSeverity.Critical,
            senderObject, callerName);
    }

    public ILogView AsReadOnly()
    {
        return new ReadOnlyLogView(this);
    }

    public void Clear()
    {
        while (_Log.TryTake(out _))
        {
        }
    }

    public void FlushFrom(ILogView logView, bool silent = false)
    {
        foreach (var message in logView.Messages)
        {
            _Log.Add(message);

            if (!silent) MessageAdded?.Invoke(this, message);
        }
    }
}