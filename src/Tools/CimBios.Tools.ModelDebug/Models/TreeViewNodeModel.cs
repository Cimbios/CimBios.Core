using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Media.Imaging;

namespace CimBios.Tools.ModelDebug.Models;

public class TreeViewNodeModel : INotifyPropertyChanged
{
    public TreeViewNodeModel[] SubNodes { get => _SubNodes.ToArray(); }
    public TreeViewNodeModel? ParentNode 
    { 
        get => _ParentNode; 
        set
        {
            if (_ParentNode == value)
            {
                return;
            } 

            _ParentNode = value;
            _ParentNode?.AddChild(this);

            PropertyChanged?.Invoke(this, 
                new PropertyChangedEventArgs(nameof(ParentNode)));
        } 
    }

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

    public bool IsVisible
    { 
        get => _IsVisible; 
        set
        {
            _IsVisible = value;
            PropertyChanged?.Invoke(this, 
                new PropertyChangedEventArgs(nameof(IsVisible)));
        }
    }

    public TreeViewNodeModel()
    {
    }

    public void AddChild(TreeViewNodeModel child)
    {
        if (child.ParentNode == this 
            && _SubNodes.Contains(child))
        {
            return;
        }

        _SubNodes.Add(child);
        child.ParentNode = this;

        PropertyChanged?.Invoke(this, 
            new PropertyChangedEventArgs(nameof(SubNodes)));
    } 

    public List<TreeViewNodeModel> _SubNodes = new List<TreeViewNodeModel>();
    public TreeViewNodeModel? _ParentNode = null;
    private string _Title = string.Empty;
    private bool _IsExpanded = false;
    private bool _IsVisible = true;

    public event PropertyChangedEventHandler? PropertyChanged;
}
