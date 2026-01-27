using System;
using CimBios.Tools.ModelDebug.Services;
using CimBios.Tools.ModelDebug.ViewModels;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace CimBios.Tools.ModelDebug;

public class ProtocolServiceSink(IFormatProvider? formatProvider) : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage(formatProvider);
        Console.WriteLine(DateTimeOffset.Now + " "  + message);

        var kind = ProtocolMessageKind.Info;
        if (logEvent.Level == LogEventLevel.Error)
            kind = ProtocolMessageKind.Error;
        else if (logEvent.Level == LogEventLevel.Warning)
            kind = ProtocolMessageKind.Warn;
        else if (logEvent.Level == LogEventLevel.Debug)
            kind = ProtocolMessageKind.Debug;

        var source = string.Empty;
        if (logEvent.Properties.TryGetValue("SourceContext", out var sourceValue))
            source = sourceValue.ToString();
        
        GlobalServices.ProtocolService.AddMessage(
            new ProtocolMessage(
                DateTimeOffset.Now.ToString("hh:mm:ss t") + " "  + message, 
                source,
                kind)
            );
    }
}

public static class ProtocolServiceSinkExtensions
{
    public static LoggerConfiguration ProtocolServiceSink(
        this LoggerSinkConfiguration loggerConfiguration,
        IFormatProvider? formatProvider = null)
    {
        return loggerConfiguration.Sink(new ProtocolServiceSink(formatProvider));
    }
}
