using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using CimBios.Core.CimModel.Context;
using CimBios.Tools.ModelDebug.Models;
using CommunityToolkit.Mvvm.Input;

namespace CimBios.Tools.ModelDebug.ViewModels;

public class CimObjectsObserverViewModel : TreeViewModelBase
{
    public override IEnumerable<CimObjectDataTreeModel> Nodes
    { 
        get
        {
            return _NodesCache;
        }
    }

    public HierarchicalTreeDataGridSource<TreeViewNodeModel> 
    CimObjectsSource { get; }

    public AsyncRelayCommand ExpandAllNodesCommand { get; }
    public AsyncRelayCommand UnexpandAllNodesCommand { get; }

    private ModelContext? _CimModelContext { get; set; }

    public CimObjectsObserverViewModel()
    {
        CimObjectsSource = new 
        HierarchicalTreeDataGridSource<TreeViewNodeModel>(_NodesCache)
        {
            // Columns = 
            // {
            //     new HierarchicalExpanderColumn<TreeViewNodeModel>(
            //             new TextColumn<TreeViewNodeModel, string>
            //                 ("uuid", x => (x as CimObjectDataTreeModel) == null ? 
            //                     (x as CimObjectDataTreeModel)!.Uuid : x.Title), 
            //                 x => x.SubNodes.OfType<TreeViewNodeModel>()),
            //         new TextColumn<TreeViewNodeModel, string>
            //                 ("name", x => (x as CimObjectDataTreeModel) == null ? 
            //                     (x as CimObjectDataTreeModel)!.Uuid : x.Title),
            // }
        };

        ExpandAllNodesCommand = new AsyncRelayCommand
            (() => DoExpandAllNodes(true));

        UnexpandAllNodesCommand = new AsyncRelayCommand
            (() => DoExpandAllNodes(false));

        SubscribeModelContextLoad();
    }

    private void SubscribeModelContextLoad()
    {
        if (Services.ServiceLocator.GetInstance()
            .TryGetService<ModelContext>(out var modelContext) == false
            || modelContext == null)
        {
            return;
        }

        modelContext.ModelLoaded += ModelContext_ModelLoaded;
    }

    private void ModelContext_ModelLoaded(object? sender, EventArgs e)
    {
        if (sender is ModelContext modelContext == false)
        {
            return;
        }

        _CimModelContext = modelContext;
        FillData();
    }

    private void FillData()
    {
        _NodesCache.Clear();

        if (_CimModelContext == null)
        {
            return;
        }

        foreach (var cimObj in _CimModelContext.GetAllObjects())
        {
            _NodesCache.Add(new CimObjectDataTreeModel(cimObj));
        }

        var a = _NodesCache.Cast<CimObjectDataTreeModel>();

        OnPropertyChanged(nameof(CimObjectsSource));
    }

    protected ObservableCollection<CimObjectDataTreeModel> _NodesCache
        = new ObservableCollection<CimObjectDataTreeModel>();
}
