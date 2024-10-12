using System.Collections.Generic;
using Avalonia.Media.Imaging;

namespace CimBios.Tools.ModelDebug.Models;

public class TreeViewNodeModel : LinkedNodeModel
{
    public string Title
    { 
        get => _Title; 
        set
        {
            _Title = value;
            OnPropertyChanged(nameof(Title));
        }
    }

    public string Description
    { 
        get => _Description; 
        set
        {
            _Description = value;
            OnPropertyChanged(nameof(Description));
        }
    }

    public Bitmap? Image { get; set; }

    public bool IsExpanded 
    { 
        get => _IsExpanded; 
        set
        {
            _IsExpanded = value;
            OnPropertyChanged(nameof(IsExpanded));
        }
    }

    public bool IsVisible
    { 
        get => _IsVisible; 
        set
        {
            _IsVisible = value;
            OnPropertyChanged(nameof(IsVisible));
        }
    }

    public TreeViewNodeModel()
        : base()
    {
    }

    private Dictionary<string, object> _Fields 
        = new Dictionary<string, object>();

    private bool _IsExpanded = false;
    private bool _IsVisible = true;
    private string _Title = string.Empty;
    private string _Description = string.Empty;
}
