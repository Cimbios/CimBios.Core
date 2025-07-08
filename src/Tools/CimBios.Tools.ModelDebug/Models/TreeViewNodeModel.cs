using Avalonia.Media.Imaging;

namespace CimBios.Tools.ModelDebug.Models;

public class TreeViewNodeModel : LinkedNodeModel
{
    private string _Description = string.Empty;

    private bool _IsExpanded;
    private bool _IsVisible = true;
    private string _Title = string.Empty;

    public string Title
    {
        get => _Title;
        set
        {
            _Title = value;
            OnPropertyChanged();
        }
    }

    public string Description
    {
        get => _Description;
        set
        {
            _Description = value;
            OnPropertyChanged();
        }
    }

    public Bitmap? Image { get; set; }

    public bool IsExpanded
    {
        get => _IsExpanded;
        set
        {
            _IsExpanded = value;
            OnPropertyChanged();
        }
    }

    public bool IsVisible
    {
        get => _IsVisible;
        set
        {
            _IsVisible = value;
            OnPropertyChanged();
        }
    }
}