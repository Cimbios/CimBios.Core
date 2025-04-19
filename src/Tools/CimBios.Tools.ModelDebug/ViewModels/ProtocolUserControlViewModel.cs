using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using CimBios.Tools.ModelDebug.Models;
using CimBios.Tools.ModelDebug.Services;

namespace CimBios.Tools.ModelDebug.ViewModels;

public class ProtocolViewModel : ViewModelBase
{
    public HierarchicalTreeDataGridSource<TreeViewNodeModel> DataSource 
    { get; }

    private ProtocolService _ProtocolService  
    {
        get
        {
            if (ServiceLocator.GetInstance().TryGetService<ProtocolService>(
                out var protocolService) == false || protocolService == null)
            {
                throw new NotSupportedException(
                    "Protocol service has not been initialized!");
            }

            return protocolService;
        }
    }

    public ProtocolViewModel()
    {
        DataSource = new 
        HierarchicalTreeDataGridSource<TreeViewNodeModel>(_Cache)
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

        _ProtocolService.OnMessageAdded += ProtocolServiceMessageAdded;

        _ProtocolService.Info("Protocol view intialized.", "Protocol");
    }

    public void ClearView()
    {
        _Cache.Clear();
        _GroupMap.Clear();
    }

    public void SaveToFile()
    {
        var path = $"{Directory.GetCurrentDirectory()}\\log-{DateTime.Now.ToFileTime()}.csv";

        _ProtocolService.SaveToFile(path);

        _Cache.Add(new ProtocolMessageModel(
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
            _Cache.Add(newProtocolMessage);
            return;
        }

        if (_GroupMap.TryGetValue(pe.Message.GroupDescriptor, 
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

            _Cache.Add(newGroupNode);
            _GroupMap.Add(pe.Message.GroupDescriptor, newGroupNode);
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
            return pmm.Message.Source.ToString();
        }

        return "[Unk]";
    }

    private readonly ObservableCollection<TreeViewNodeModel> _Cache = [];
    private readonly Dictionary<GroupDescriptor, TreeViewNodeModel> _GroupMap = [];
}
