using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using CimBios.Tools.ModelDebug.Models;
using CimBios.Tools.ModelDebug.Services;

namespace CimBios.Tools.ModelDebug.ViewModels;

public class ProtocolViewModel : ViewModelBase
{
    public HierarchicalTreeDataGridSource<TreeViewNodeModel> DataSource 
    { get; }
    
    public ProtocolViewModel()
    {
        DataSource = new 
        HierarchicalTreeDataGridSource<TreeViewNodeModel>(_cache)
        {
            Columns = 
            {
                new HierarchicalExpanderColumn<TreeViewNodeModel>(
                    new TextColumn<TreeViewNodeModel, string>("Type", 
                        x => SelectType(x)), 
                    x => x.SubNodes.OfType<TreeViewNodeModel>(), 
                    x => x.SubNodes.Count != 0, 
                    x => x.IsExpanded),
                new TextColumn<TreeViewNodeModel, string>("Source", x => SelectSource(x)),
                new TextColumn<TreeViewNodeModel, string>("Text", x => x.Title),
            }
        };

        GlobalServices.ProtocolService.OnMessageAdded 
            += ProtocolServiceMessageAdded;

        GlobalServices.ProtocolService.Info(
            "Protocol view initialized.", "Protocol");
    }

    public void ClearView()
    {
        _cache.Clear();
        _groupMap.Clear();
    }

    public void SaveToFile()
    {
        var path = $"{Directory.GetCurrentDirectory()}\\log-{DateTime.Now.ToFileTime()}.csv";

        GlobalServices.ProtocolService.SaveToFile(path);

        _cache.Add(new ProtocolMessageModel(
            new ProtocolMessage($"[S] Log saved to {path}", "Protocol", 
            ProtocolMessageKind.Info)
        ));
    }

    private void ProtocolServiceMessageAdded(object? sender, EventArgs e)
    {
        if (e is not ProtocolNewMessageEventArgs pe)
        {
            return;
        }

        var newProtocolMessage = new ProtocolMessageModel(pe.Message);
        if (pe.Message.GroupDescriptor == null)
        {
            _cache.Add(newProtocolMessage);
            return;
        }

        if (_groupMap.TryGetValue(pe.Message.GroupDescriptor, 
            out var groupNode))
        {
            groupNode.AddChild(newProtocolMessage);
        }
        else
        {
            var newGroupNode = new TreeViewNodeModel
            {
                Title = pe.Message.GroupDescriptor.Description
            };
            newGroupNode.AddChild(newProtocolMessage);

            _cache.Add(newGroupNode);
            _groupMap.Add(pe.Message.GroupDescriptor, newGroupNode);
        }
    }

    private string SelectType(TreeViewNodeModel node)
    {
        if (node is ProtocolMessageModel pmm)
        {
            return $"[{pmm.Message.Kind}]";
        }

        var filter = node.SubNodes.OfType<ProtocolMessageModel>();
        if (filter.Any())
        {
            var maxKind = node.SubNodes.OfType<ProtocolMessageModel>()
                .Select(x => x.Message.Kind).Max();

            return $"[{maxKind}]";
        }
        
        return "[Unk]";
    }

    private string SelectSource(TreeViewNodeModel node)
    {
        if (node is ProtocolMessageModel pmm)
        {
            return pmm.Message.Source;
        }

        return "[Unk]";
    }

    private readonly ObservableCollection<TreeViewNodeModel> _cache = [];
    private readonly Dictionary<GroupDescriptor, TreeViewNodeModel> _groupMap = [];
}
