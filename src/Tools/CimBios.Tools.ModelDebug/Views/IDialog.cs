using System.Threading.Tasks;
using Avalonia.Controls;

namespace CimBios.Tools.ModelDebug.Views;

/// <summary>
/// 
/// </summary>
public interface IDialog
{
    public IDialogResult Result { get; }

    public Task Show(Window owner, params object[]? args);
}

/// <summary>
/// 
/// </summary>
public interface IDialogResult
{
    public bool Succeed { get; }
}

/// <summary>
/// 
/// </summary>
public class FailedDialogResult : IDialogResult
{
    public bool Succeed => false;
}
