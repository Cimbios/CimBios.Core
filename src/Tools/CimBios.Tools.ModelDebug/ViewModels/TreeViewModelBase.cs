using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CimBios.Tools.ModelDebug.Models;

namespace CimBios.Tools.ModelDebug.ViewModels;

public abstract class TreeViewModelBase : ViewModelBase
{
    public abstract IEnumerable<TreeViewNodeModel> Nodes { get; }

    public TreeViewNodeModel? SelectedItem { 
        get => _SelectedItem; 
        set
        {
            _SelectedItem = value;
            OnPropertyChanged(nameof(SelectedItem));   
        }
    }

    protected void ApplyFilter(Func<TreeViewNodeModel, bool> filterDelegate)
    {
        var nodesStack = new Stack<TreeViewNodeModel>(Nodes);

        var visited = new HashSet<TreeViewNodeModel>();
        while (nodesStack.TryPop(out var node))
        {
            node.IsVisible = true;

            if (filterDelegate(node) == true)
            {
                visited.Add(node);

                var parent = node.ParentNode as TreeViewNodeModel;
                while (parent != null)
                {
                    visited.Add(parent);
                    parent.IsVisible = true;
                    parent.IsExpanded = true;
                    parent = parent.ParentNode as TreeViewNodeModel;
                }
            }
            else
            {
                if (visited.Contains(node) == false)
                {
                    node.IsVisible = false;
                }
            }

            node.SubNodes.OfType<TreeViewNodeModel>()
                .ToList().ForEach(n => nodesStack.Push(n));
        }     

        OnPropertyChanged(nameof(Nodes));
    }

    protected Task DoExpandAllNodes(bool IsExpand)
    {
        var nodesStack = new Stack<TreeViewNodeModel>(Nodes);

        while (nodesStack.TryPop(out var node))
        {
            node.IsExpanded = IsExpand;
            node.SubNodes.OfType<TreeViewNodeModel>()
                .ToList().ForEach(n => nodesStack.Push(n));
        }

        return Task.CompletedTask;
    }

    private TreeViewNodeModel? _SelectedItem;
}
