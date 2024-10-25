using CimBios.Utils.ClassTraits;

namespace CimBios.Tests.ClassTraits;

internal class CanLog : ICanLog
{
    public ILogView Log => _Log;

    public CanLog()
    {
        _Log = new PlainLogView(this);
    }

    public ILogMessage Do(LogMessageSeverity severity)
    {
        return _Log.NewMessage("Test", severity);
    }

    private PlainLogView _Log;
}

public class CanLogFeatTest
{
    [Fact]
    public void EmptyLog()
    {
        Assert.Empty(GetCanLog().Log.Log);
    }

    [Fact]
    public void LogSourceAssert()
    {
        var canLog = GetCanLog();

        Assert.Equal(canLog, canLog.Log.Source);
    }

    [Theory]
    [InlineData(LogMessageSeverity.None)]
    [InlineData(LogMessageSeverity.Info)]
    [InlineData(LogMessageSeverity.Warning)]
    [InlineData(LogMessageSeverity.Error)]
    [InlineData(LogMessageSeverity.Critical)]
    public void NewMessageWithSeverity(LogMessageSeverity messageSeverity)
    {
        var logMessage = GetCanLog().Do(messageSeverity);
        Assert.Equal(messageSeverity, logMessage.Severity);
    } 

    [Fact]
    public void MessageAdded()
    {
        var canLog = GetCanLog();

        bool raised = false;
        canLog.Log.MessageAdded += (sender, args) => raised = true;
        canLog.Do(LogMessageSeverity.None);

        Assert.True(raised);
    }

    private CanLog GetCanLog()
    {
        return new CanLog();
    }
}