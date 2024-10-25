namespace CimBios.Utils.ClassTraits;

/// <summary>
/// Can log interface - provides class logger functionality.
/// </summary>
public interface ICanLog
{
    /// <summary>
    /// Log messages view interface property.
    /// </summary>
    public ILogView Log { get; }
}

/// <summary>
/// Log messages view interface. Provides log messages access.
/// </summary>
public interface ILogView
{
    /// <summary>
    /// View of log message collection. Should be concurrent based.
    /// </summary>
    public IReadOnlyCollection<ILogMessage> Log { get; } 

    /// <summary>
    /// Log messages source. Can be assembly, class instance or anything else.
    /// </summary>
    public object Source { get; }

    /// <summary>
    /// Set debug mode - log debug messages.
    /// </summary>
    public bool DebugLogMode { get; set; }

    /// <summary>
    /// Event fires on new message added.
    /// </summary>
    public event MessageAddedEventHandler? MessageAdded;
    public delegate void MessageAddedEventHandler(ILogView sender, 
        ILogMessage message);
}