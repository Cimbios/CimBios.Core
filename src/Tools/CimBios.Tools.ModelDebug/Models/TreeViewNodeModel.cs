using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;

namespace CimBios.Tools.ModelDebug.Models;

public class TreeViewNodeModel
{
    public ObservableCollection<TreeViewNodeModel> SubNodes { get; }
    public string Title { get; set; }
    public Bitmap? Image { get; set; }
    public bool IsExpanded { get; set; }

    public TreeViewNodeModel()
    {
        Title = string.Empty;
        SubNodes = new ObservableCollection<TreeViewNodeModel>();
    }
}
