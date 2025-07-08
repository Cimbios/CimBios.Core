using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CimBios.Tools.ModelDebug.Models;

public class LinkedNodeModel : ObservableObject
{
    private readonly List<LinkedNodeModel> _SubNodes = [];
    private LinkedNodeModel? _ParentNode;

    protected LinkedNodeModel()
    {
    }

    public IReadOnlyCollection<LinkedNodeModel> SubNodes
        => _SubNodes.AsReadOnly();

    public LinkedNodeModel? ParentNode
    {
        get => _ParentNode;
        set
        {
            if (_ParentNode == value) return;

            if (value == null)
                _ParentNode?.RemoveChild(this);
            else
                _ParentNode?.AddChild(this);

            _ParentNode = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(SubNodes));
        }
    }

    public void AddChild(LinkedNodeModel child)
    {
        if (child.ParentNode == this
            && _SubNodes.Contains(child))
            return;

        _SubNodes.Add(child);
        child.ParentNode = this;

        OnPropertyChanged(nameof(ParentNode));
        OnPropertyChanged(nameof(SubNodes));
    }

    public void RemoveChild(LinkedNodeModel child)
    {
        if (_SubNodes.Contains(child))
        {
            _SubNodes.Remove(child);
            child.ParentNode = null;

            OnPropertyChanged(nameof(ParentNode));
            OnPropertyChanged(nameof(SubNodes));
        }
    }
}