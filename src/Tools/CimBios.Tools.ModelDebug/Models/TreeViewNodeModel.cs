using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;

namespace CimBios.Tools.ModelDebug.Models;

public class TreeViewNodeModel
{
    public ObservableCollection<TreeViewNodeModel> SubNodes { get; }
    public string Title { get; }
    public Bitmap? Image { get; set; }

    public TreeViewNodeModel(string title)
    {
        Title = title;
        SubNodes = new ObservableCollection<TreeViewNodeModel>();
    }

    public TreeViewNodeModel(string title, 
        ObservableCollection<TreeViewNodeModel> subNodes)
    {
        Title = title;
        SubNodes = subNodes;
    }
}
