namespace CimBios.Utils.ClassTraits.CanLog;

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
