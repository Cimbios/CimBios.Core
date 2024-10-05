using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using CimBios.Core.CimModel.Context;
using CimBios.Core.RdfXmlIOLib;
using CimBios.Tools.ModelDebug.Models;
using CommunityToolkit.Mvvm.Input;

namespace CimBios.Tools.ModelDebug.ViewModels;

public class CimObjectsObserverViewModel : TreeViewModelBase
{
    public override IEnumerable<TreeViewNodeModel> Nodes 
    {  get => _NodesCache; }

    public HierarchicalTreeDataGridSource<TreeViewNodeModel> CimObjectsSource 
    { get; }

    public AsyncRelayCommand ExpandAllNodesCommand { get; }
    public AsyncRelayCommand UnexpandAllNodesCommand { get; }
    
    private ModelContext? _CimModelContext { get; set; }

    public string SearchString 
    { 
        get => _SearchString; 
        set
        {
            _SearchString = value;
            OnPropertyChanged(nameof(SearchString));

        }
    } 

    public CimObjectsObserverViewModel()
    {
        CimObjectsSource = new 
        HierarchicalTreeDataGridSource<TreeViewNodeModel>(_NodesCache)
        {
            Columns = 
            {
                new HierarchicalExpanderColumn<TreeViewNodeModel>(
                    new TextColumn<TreeViewNodeModel, string>("Title", 
                        x => x.Title), 
                    x => x.SubNodes.Cast<TreeViewNodeModel>(), 
                    null, 
                    x => x.IsExpanded),
            }
        };

        CimObjectsSource.RowSelection!.SingleSelect = true;

        CimObjectsSource.RowSelection!.SelectionChanged 
            += CellSelection_SelectionChanged;

        ExpandAllNodesCommand = new AsyncRelayCommand
            (() => DoExpandAllNodes(true));

        UnexpandAllNodesCommand = new AsyncRelayCommand
            (() => DoExpandAllNodes(false));

        SubscribeModelContextLoad();
    }

    private void CellSelection_SelectionChanged(object? sender, 
        TreeSelectionModelSelectionChangedEventArgs<TreeViewNodeModel> e)
    {
        if (CimObjectsSource.RowSelection == null)
        {
            SelectedItem = null;
            return;
        }

        SelectedItem = e.SelectedItems.FirstOrDefault();
    }

    public Task Foo(TreeDataGrid? dataGrid)
    {   
        CimObjectsSource.RowSelection!.Select(CimObjectsSource.Items.Count()-1);
        dataGrid!.RowsPresenter!.BringIntoView(CimObjectsSource.Items.Count()-1);
        
        return Task.CompletedTask;
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

        var schemaClassesUri = 
            new Dictionary<Uri, TreeViewNodeModel>(new RdfUriComparer());

        foreach (var cimObj in _CimModelContext.GetAllObjects())
        {
            var cimObjNode = new CimObjectDataTreeModel(cimObj);

            var classUri = cimObj.ObjectData.ClassType;
            if (schemaClassesUri.TryGetValue(classUri, out var classNode))
            {
                classNode.AddChild(cimObjNode);
            }
            else
            {
                var newClassNode = new TreeViewNodeModel() 
                    { Title = classUri.AbsoluteUri };
                
                newClassNode.AddChild(cimObjNode);

                _NodesCache.Add(newClassNode);
                schemaClassesUri.Add(classUri, newClassNode);
            }
        }
    }

    private string _SearchString = string.Empty;

    private ObservableCollection<TreeViewNodeModel> _NodesCache 
        = new ObservableCollection<TreeViewNodeModel>();

}
