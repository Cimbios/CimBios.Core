using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CimBios.Utils.ClassTraits.CanLog;

namespace CimBios.Tools.ModelDebug.Services;

public class ProtocolService
{
    public IReadOnlyCollection<ProtocolMessage> Messages
        => _Messages.AsReadOnly();

    public event EventHandler? OnMessageAdded;

    public void SaveToFile(string path)
    {
        using var writer = new StreamWriter(path);
        foreach (var message in Messages)
        {
            writer.WriteLine(
                $"{message.Kind}; {message.Source}; {message.Text}"
            );
        }
    }

    public void Info(string text, string source, 
        GroupDescriptor? groupDescriptor = null)
    {
        NewMessage(text, source, ProtocolMessageKind.Info, groupDescriptor);
    }

    public void Warn(string text, string source, 
        GroupDescriptor? groupDescriptor = null)
    {
        NewMessage(text, source, ProtocolMessageKind.Warn, groupDescriptor);
    }

    public void Error(string text, string source, 
        GroupDescriptor? groupDescriptor = null)
    {
        NewMessage(text, source, ProtocolMessageKind.Error, groupDescriptor);
    }

    public void AddMessage(ProtocolMessage message)
    {
        _Messages.Add(message);

        OnMessageAdded?.Invoke(this, new ProtocolNewMessageEventArgs(message));
    }

    private void NewMessage(string text, string source, 
        ProtocolMessageKind kind, GroupDescriptor? groupDescriptor = null)
    {
        var message = new ProtocolMessage(text, source, kind, groupDescriptor);
        AddMessage(message);
    }

    private readonly List<ProtocolMessage> _Messages = [];
}

public class ProtocolMessage(string text, string source,
    ProtocolMessageKind kind, GroupDescriptor? groupDescriptor = null)
{
    public GroupDescriptor? GroupDescriptor { get; } = groupDescriptor;

    public string Text { get; } = text;

    public string Source { get; } = source;

    public ProtocolMessageKind Kind { get; } = kind;
}

public class GroupDescriptor (string description) 
    : IEqualityComparer<GroupDescriptor>
{
    public Guid Uuid { get; } = Guid.NewGuid();
    public string Description { get; } = description;


    public bool Equals(GroupDescriptor? x, GroupDescriptor? y)
    {
        if (x == null && y == null)
        {
            return true;
        }

        if (x != null)
        {
            return x.Uuid.Equals(y);
        }

        if (y != null)
        {
            return y.Uuid.Equals(x);
        }

        return false;
    }

    public int GetHashCode([DisallowNull] GroupDescriptor obj)
    {
        return obj.Uuid.GetHashCode();
    }
}

public class ProtocolNewMessageEventArgs (ProtocolMessage message)
    : EventArgs
{
    public ProtocolMessage Message { get; } = message;
}

public enum ProtocolMessageKind
{
    Info,
    Warn,
    Error
}

public static class CanLogMessagesConverter
{
    public static ProtocolMessage Convert(ILogMessage logMessage,
        GroupDescriptor? groupDescriptor = null)
    {
        var kind = ProtocolMessageKind.Info;
        switch (logMessage.Severity)
        {
            case LogMessageSeverity.Warning:
                kind = ProtocolMessageKind.Warn;
                break;
            case LogMessageSeverity.Error:
            case LogMessageSeverity.Critical:
                kind = ProtocolMessageKind.Error;
                break;
        }

        var pm = new ProtocolMessage(logMessage.Text, 
            logMessage.CallerName, kind, groupDescriptor);

        return pm;
    }
}