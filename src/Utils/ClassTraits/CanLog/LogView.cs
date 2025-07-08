namespace CimBios.Utils.ClassTraits.CanLog;

/// <summary>
///     Log messages view interface. Provides log messages access.
/// </summary>
public interface ILogView
{
    public delegate void MessageAddedEventHandler(ILogView sender,
        ILogMessage message);

    /// <summary>
    ///     View of log message collection. Should be concurrent based.
    /// </summary>
    public IReadOnlyCollection<ILogMessage> Messages { get; }

    /// <summary>
    ///     Log messages source. Can be assembly, class instance or anything else.
    /// </summary>
    public object Source { get; }

    /// <summary>
    ///     Event fires on new message added.
    /// </summary>
    public event MessageAddedEventHandler? MessageAdded;
}

public sealed class ReadOnlyLogView
    : ILogView
{
    public ReadOnlyLogView(ILogView innerLogView)
    {
        _InnerLogView = innerLogView;
        innerLogView.MessageAdded += (s, e) => MessageAdded?.Invoke(s, e);
    }

    private ILogView _InnerLogView { get; }

    public IReadOnlyCollection<ILogMessage> Messages => _InnerLogView.Messages;
    public object Source => _InnerLogView.Source;

    public event ILogView.MessageAddedEventHandler? MessageAdded;
}