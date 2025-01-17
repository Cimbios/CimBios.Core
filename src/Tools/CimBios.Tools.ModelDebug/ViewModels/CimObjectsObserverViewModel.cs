using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Document;
using CimBios.Core.RdfIOLib;
using CimBios.Tools.ModelDebug.Models;
using CimBios.Tools.ModelDebug.Services;
using CommunityToolkit.Mvvm.Input;

namespace CimBios.Tools.ModelDebug.ViewModels;

public class CimObjectsObserverViewModel : TreeViewModelBase
{
    public override IEnumerable<TreeViewNodeModel> Nodes 
    {  get => _NodesCache; }

    public HierarchicalTreeDataGridSource<TreeViewNodeModel> CimObjectsSource 
    { get; }

    public HierarchicalTreeDataGridSource<CimObjectPropertyModel> PropertySource 
    { get; }

    public AsyncRelayCommand ExpandAllNodesCommand { get; }
    public AsyncRelayCommand UnexpandAllNodesCommand { get; }
    
    private CimDocument? _CimModelDocument { get; set; }

    public string SearchString 
    { 
        get => _SearchString; 
        set
        {
            _SearchString = value;
            OnPropertyChanged(nameof(SearchString));

        }
    } 

    public string SelectedUuid 
    { 
        get => _SelectedUuid; 
        set
        {
            _SelectedUuid = value;
            OnPropertyChanged(nameof(SelectedUuid));

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

        PropertySource = new HierarchicalTreeDataGridSource<CimObjectPropertyModel>(_PropCache)
        {
            Columns =
            {
                new HierarchicalExpanderColumn<CimObjectPropertyModel>(
                    new TextColumn<CimObjectPropertyModel, string>
                        ("Name", x => x.Name),
                    x => x.SubNodes.Cast<CimObjectPropertyModel>()),
                new TextColumn<CimObjectPropertyModel, string>
                    ("Value", x => x.Value),
            },
        };

        PropertySource.RowSelection!.SingleSelect = true;

        ExpandAllNodesCommand = new AsyncRelayCommand
            (() => DoExpandAllNodes(true));

        UnexpandAllNodesCommand = new AsyncRelayCommand
            (() => DoExpandAllNodes(false));

        SubscribeModelContextLoad();
    }

    public void Find(TreeDataGrid? dataGrid)
    {   
        if (SearchString == string.Empty
            || dataGrid == null)
        {
            return;
        }

        int classRow = 0;
        foreach (var item in CimObjectsSource.Items)
        {
            int objectRow = 0;
            foreach (var subItem in item.SubNodes.OfType<TreeViewNodeModel>())
            {
                if (subItem.Title.Contains(SearchString))
                {
                    var idx = new IndexPath([classRow, objectRow]);
                    CimObjectsSource.Expand(idx);
                    var rowId = dataGrid.Rows!.ModelIndexToRowIndex(idx);
                    dataGrid.RowsPresenter!.BringIntoView(rowId);
                    CimObjectsSource.RowSelection!.Select(idx);

                    return;
                }

                ++objectRow;
            }

            ++classRow;
        }
       
        return;
    }

    public void Navigate(TreeDataGrid? dataGrid)
    {
        if (PropertySource.RowSelection!.SelectedItem 
            is not CimObjectPropertyModel selectedProp)
        {
            return;
        }

        var mObj = _CimModelDocument?.GetObject(selectedProp.Value);
        if (mObj != null)
        {
            var tmpSearchString = SearchString;
            SearchString = mObj.Uuid;
            Find(dataGrid);
            SearchString = tmpSearchString;
        }

        return;
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

        ShowSelectedProperties();
    }

    private void ShowSelectedProperties()
    {
        _PropCache.Clear();
        SelectedUuid = string.Empty;

        if (SelectedItem == null
            || SelectedItem is not CimObjectDataTreeModel cimObjectItem)
        {
            return;
        }

        var dataFacade = cimObjectItem.ModelObject;

        SelectedUuid = dataFacade.Uuid;

        foreach (var attrName in dataFacade.MetaClass.AllProperties.Where(p => p.PropertyKind == Core.CimModel.Schema.CimMetaPropertyKind.Attribute).Select(p => p.ShortName))
        {
            var attrValue = dataFacade.GetAttribute<object>(attrName);
            var attrValueStr = attrValue?.ToString();
            if (attrValueStr == null)
            {
                attrValueStr = "null";
            }
            else
            {
                attrValueStr += $" ({attrValue?.GetType().Name})";
            }

            var attrNode = new CimObjectPropertyModel() 
                { Name = attrName, Value = attrValueStr };
            
            if (attrValue is IModelObject compoundAttr)
            {
                 foreach (var compoundAttrName in compoundAttr.MetaClass.AllProperties.Where(p => p.PropertyKind == Core.CimModel.Schema.CimMetaPropertyKind.Attribute).Select(p => p.ShortName))
                {
                    var compoundAttrValue = compoundAttr.GetAttribute<object>(compoundAttrName);
                    var compoundAttrValueStr = compoundAttrValue?.ToString();
                    if (compoundAttrValueStr == null)
                    {
                        compoundAttrValueStr = "null";
                    }
                    else
                    {
                        compoundAttrValueStr += $" ({compoundAttrValue?.GetType().Name})";
                    }

                    var compoundAttrNode = new CimObjectPropertyModel() 
                        { Name = compoundAttrName, Value = compoundAttrValueStr };
                    
                    attrNode.AddChild(compoundAttrNode);
                }
            }

            _PropCache.Add(attrNode);
        }

        foreach (var assoc11Name in dataFacade.MetaClass.AllProperties.Where(p => p.PropertyKind == Core.CimModel.Schema.CimMetaPropertyKind.Assoc1To1).Select(p => p.ShortName))
        {
            var assoc11Ref = dataFacade.GetAssoc1To1<IModelObject>(assoc11Name);
            string assoc11RefStr = "null";
            if (assoc11Ref != null)
            {
                assoc11RefStr = assoc11Ref.Uuid;
            }

            _PropCache.Add(new CimObjectPropertyModel() 
                { Name = assoc11Name, Value = assoc11RefStr });
        }
        
        foreach (var assoc1MName in dataFacade.MetaClass.AllProperties.Where(p => p.PropertyKind == Core.CimModel.Schema.CimMetaPropertyKind.Assoc1ToM).Select(p => p.ShortName))
        {
            var assoc1MArray = dataFacade.GetAssoc1ToM(assoc1MName);
            if (assoc1MArray == null)
            {
                continue;
            }

            var assoc1MNode = new CimObjectPropertyModel() 
                { Name = assoc1MName, Value = $"Count: {assoc1MArray.Count()}" };

            foreach (var assoc1MRef in assoc1MArray.OfType<IModelObject>())
            {
                assoc1MNode.AddChild(new CimObjectPropertyModel() 
                { Name = string.Empty, Value = assoc1MRef.Uuid });
            }

            _PropCache.Add(assoc1MNode);
        }    
    }

    private void SubscribeModelContextLoad()
    {
        if (Services.ServiceLocator.GetInstance()
            .TryGetService<NotifierService>(out var notifier) == false
            || notifier == null)
        {
            return;
        }

        notifier.Fired += ModelContext_ModelLoaded;
    }

    private void ModelContext_ModelLoaded(object? sender, EventArgs e)
    {
        if (sender is CimDocument modelContext == false)
        {
            return;
        }

        _CimModelDocument = modelContext;
        FillData();
    }

    private void FillData()
    {
        _NodesCache.Clear();

        if (_CimModelDocument == null)
        {
            return;
        }

        var schemaClassesUri = 
            new Dictionary<Uri, TreeViewNodeModel>(new RdfUriComparer());

        foreach (var cimObj in _CimModelDocument.GetAllObjects())
        {
            var cimObjNode = new CimObjectDataTreeModel(cimObj);

            var classUri = cimObj.MetaClass.BaseUri;
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

    private string _SelectedUuid = string.Empty;

    private ObservableCollection<TreeViewNodeModel> _NodesCache 
        = new ObservableCollection<TreeViewNodeModel>();

    private ObservableCollection<CimObjectPropertyModel> _PropCache 
        = new ObservableCollection<CimObjectPropertyModel>();

}
