using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Media.Imaging;

namespace CimBios.Tools.ModelDebug.Models;

public class TreeViewNodeModel : LinkedNodeModel
{
    public string Description { get; set; } = string.Empty;

    public string Title 
    { 
        get => _Title; 
        set
        {
            _Title = value;
            OnPropertyChanged(nameof(Title));
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

    private string _Title = string.Empty;
    private bool _IsExpanded = false;
    private bool _IsVisible = true;
}
