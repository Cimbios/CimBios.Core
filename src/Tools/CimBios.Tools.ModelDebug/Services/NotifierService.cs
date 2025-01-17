using System;

namespace CimBios.Tools.ModelDebug.Services;

public class NotifierService
{
    public void Fire (object? caller, EventArgs args)
    {
        Fired?.Invoke(caller, args);
    }

    public event EventHandler? Fired;
}

