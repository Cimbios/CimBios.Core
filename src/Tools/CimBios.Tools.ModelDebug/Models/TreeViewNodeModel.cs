using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Media.Imaging;

namespace CimBios.Tools.ModelDebug.Models;

public class TreeViewNodeModel : INotifyPropertyChanged
{
    public ObservableCollection<TreeViewNodeModel> SubNodes { get; }
    public string Title 
    { 
        get => _Title; 
        set
        {
            _Title = value;
            PropertyChanged?.Invoke(this, 
                new PropertyChangedEventArgs(nameof(Title)));
        } 
    }

    public Bitmap? Image { get; set; }
    public bool IsExpanded 
    { 
        get => _IsExpanded; 
        set
        {
            _IsExpanded = value;
            PropertyChanged?.Invoke(this, 
                new PropertyChangedEventArgs(nameof(IsExpanded)));
        }
    }

    public TreeViewNodeModel()
    {
        SubNodes = new ObservableCollection<TreeViewNodeModel>();
    }

    private string _Title = string.Empty;
    private bool _IsExpanded = false;

    public event PropertyChangedEventHandler? PropertyChanged;
}
