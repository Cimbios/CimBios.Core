using System.Collections.Generic;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CimBios.Tools.ModelDebug.Models;

public class LinkedNodeModel : ObservableObject
{
    public LinkedNodeModel[] SubNodes { get => _SubNodes.ToArray(); }
    public LinkedNodeModel? ParentNode 
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

            OnPropertyChanged(nameof(ParentNode));
        } 
    }

    public void AddChild(LinkedNodeModel child)
    {
        if (child.ParentNode == this 
            && _SubNodes.Contains(child))
        {
            return;
        }

        _SubNodes.Add(child);
        child.ParentNode = this;

        OnPropertyChanged(nameof(SubNodes));
    } 

    protected LinkedNodeModel() {}

    private List<LinkedNodeModel> _SubNodes = new List<LinkedNodeModel>();
    private LinkedNodeModel? _ParentNode = null;
}
