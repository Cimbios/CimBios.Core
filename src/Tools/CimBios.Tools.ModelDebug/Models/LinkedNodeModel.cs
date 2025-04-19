using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CimBios.Tools.ModelDebug.Models;

public class LinkedNodeModel : ObservableObject
{
    public IReadOnlyCollection<LinkedNodeModel> SubNodes 
        => _SubNodes.AsReadOnly();

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

    private List<LinkedNodeModel> _SubNodes = [];
    private LinkedNodeModel? _ParentNode = null;
}
