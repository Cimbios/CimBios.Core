using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDataModel.Utils;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.RdfIOLib;
using CimBios.Tools.ModelDebug.Models;
using CimBios.Tools.ModelDebug.Services;
using CimBios.Tools.ModelDebug.Views;
using CommunityToolkit.Mvvm.Input;

namespace CimBios.Tools.ModelDebug.ViewModels;

public class CimObjectsObserverViewModel : TreeViewModelBase
{
    private readonly ObservableCollection<TreeViewNodeModel> _nodesCache = [];

    private string _searchString = string.Empty;

    public CimObjectsObserverViewModel(TreeDataGrid? dataGrid)
    {
        DataGridControl = dataGrid;

        CimObjectsSource = new
            HierarchicalTreeDataGridSource<TreeViewNodeModel>(_nodesCache)
            {
                Columns =
                {
                    new HierarchicalExpanderColumn<TreeViewNodeModel>(
                        new TextColumn<TreeViewNodeModel, string>("Title",
                            x => x.Title),
                        x => x.SubNodes.Cast<TreeViewNodeModel>(),
                        x => x.SubNodes.Count != 0,
                        x => x.IsExpanded)
                }
            };

        CimObjectsSource.RowSelection!.SingleSelect = true;
        CimObjectsSource.RowSelection!.SelectionChanged
            += CellSelectionOnSelectionChanged;

        ForwardNavigateCommand = new RelayCommand(ForwardNavigate,
            () => ForwardObjectsStack.Count != 0);

        BackNavigateCommand = new RelayCommand(BackNavigate,
            () => ReverseObjectsStack.Count != 0);

        SubscribeModelContextLoad();

        var navigationService = new NavigationService(this);
        ServiceLocator.GetInstance().RegisterService(navigationService);
    }

    public override IEnumerable<TreeViewNodeModel> Nodes => _nodesCache;

    public HierarchicalTreeDataGridSource<TreeViewNodeModel> CimObjectsSource { get; set; }

    public RelayCommand BackNavigateCommand { get; }
    public RelayCommand ForwardNavigateCommand { get; }

    private Stack<IModelObject> ForwardObjectsStack { get; } = [];
    private Stack<IModelObject> ReverseObjectsStack { get; } = [];

    private TreeDataGrid? DataGridControl { get; }
    private ICimDataModel? CimModelDocument { get; set; }

    public string SearchString
    {
        get => _searchString;
        set
        {
            _searchString = value;
            OnPropertyChanged();
        }
    }

    private void CellSelectionOnSelectionChanged(object? sender,
        TreeSelectionModelSelectionChangedEventArgs<TreeViewNodeModel> e)
    {
        var selectedItem = e.SelectedItems.FirstOrDefault();
        var deselectedItem = e.DeselectedItems.FirstOrDefault();

        if (selectedItem is not CimObjectDataTreeModel selectedCimObjectItem
            || deselectedItem is not CimObjectDataTreeModel deselectedCimObjectItem)
        {
            BackNavigateCommand.NotifyCanExecuteChanged();
            ForwardNavigateCommand.NotifyCanExecuteChanged();

            return;
        }

        if (ReverseObjectsStack.TryPeek(out var lastBack)
            && lastBack == selectedCimObjectItem.ModelObject)
        {
            ReverseObjectsStack.Pop();
            ForwardObjectsStack.Push(deselectedCimObjectItem.ModelObject);

            BackNavigateCommand.NotifyCanExecuteChanged();
            ForwardNavigateCommand.NotifyCanExecuteChanged();

            return;
        }

        if (ForwardObjectsStack.TryPeek(out var lastForward)
            && lastForward == selectedCimObjectItem.ModelObject)
            ForwardObjectsStack.Pop();

        ReverseObjectsStack.Push(deselectedCimObjectItem.ModelObject);

        BackNavigateCommand.NotifyCanExecuteChanged();
        ForwardNavigateCommand.NotifyCanExecuteChanged();
    }

    private void ForwardNavigate()
    {
        if (ForwardObjectsStack.TryPeek(out var obj)) Find(obj.OID.ToString());
    }

    private void BackNavigate()
    {
        if (ReverseObjectsStack.TryPeek(out var obj)) Find(obj.OID.ToString());
    }

    public void Find(string searchString)
    {
        if (searchString == string.Empty
            || DataGridControl == null)
            return;

        var classRow = 0;
        foreach (var item in CimObjectsSource.Items)
        {
            var objectRow = 0;
            foreach (var subItem in item.SubNodes.OfType<TreeViewNodeModel>())
            {
                if (subItem.Title.Contains(searchString.Trim(),
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    var idx = new IndexPath(classRow, objectRow);
                    CimObjectsSource.Expand(idx);
                    CimObjectsSource.RowSelection!.Select(idx);
                    DataGridControl.InvalidateArrange();
                    DataGridControl.InvalidateVisual();
                    var rowId = DataGridControl.Rows!.ModelIndexToRowIndex(idx);
                    DataGridControl.RowsPresenter!.BringIntoView(rowId);
                    DataGridControl.TryGetRow(rowId)?.Focus();

                    return;
                }

                ++objectRow;
            }

            ++classRow;
        }
    }

    public async Task LoadModel()
    {
        var result = await GlobalServices.DialogService
            .ShowDialog<CimModelOpenSaveWindow>(
                CimModelOpenSaveWindow.DialogMode.Load);

        if (result is not CimModelOpenSaveResult openSaveResult
            || !openSaveResult.Succeed) return;

        GlobalServices.LoaderService.LoadModelFromFile(
            openSaveResult.ModelPath,
            openSaveResult.SchemaPath,
            openSaveResult.DescriptorFactory,
            openSaveResult.SchemaFactory,
            openSaveResult.RdfSerializerFactory,
            openSaveResult.SerializerSettings,
            out _
        );
    }

    public async Task SaveModel()
    {
        var result = await GlobalServices.DialogService
            .ShowDialog<CimModelOpenSaveWindow>(
                CimModelOpenSaveWindow.DialogMode.Save);

        if (result is not CimModelOpenSaveResult openSaveResult
            || !openSaveResult.Succeed) return;

        GlobalServices.LoaderService.SaveModelToFile(
            openSaveResult.ModelPath,
            openSaveResult.SchemaPath,
            openSaveResult.SchemaFactory,
            openSaveResult.RdfSerializerFactory,
            openSaveResult.SerializerSettings,
            out _
        );
    }

    public void RemoveSelectedObject()
    {
        if (GlobalServices.NavigationService.SelectedObject == null
            || CimModelDocument == null)
            return;

        CimModelDocument.RemoveObject(
            GlobalServices.NavigationService.SelectedObject);
    }

    public void ImportDiff()
    {
    }

    public async Task CreateNewObject()
    {
        if (CimModelDocument == null) return;

        var result = await GlobalServices.DialogService
            .ShowDialog<CimObjectCreatorDialog>();

        if (result is not CimObjectCreatorResult creatorResult
            || !creatorResult.Succeed) return;

        CimModelDocument.CreateObject(creatorResult.Descriptor,
            creatorResult.MetaClass);
    }

    private void SubscribeModelContextLoad()
    {
        GlobalServices.LoaderService.PropertyChanged += ModelContext_ModelLoaded;
    }

    private void ModelContext_ModelLoaded(object? sender, EventArgs e)
    {
        ReverseObjectsStack.Clear();
        ForwardObjectsStack.Clear();

        if (sender is not CimModelLoaderService loaderService) return;

        CimModelDocument = loaderService.DataContext;

        if (CimModelDocument == null) return;

        LoadObjectsCache();

        CimModelDocument.ModelObjectStorageChanged
            += CimModelDocumentOnModelObjectStorageChanged;
    }

    private void CimModelDocumentOnModelObjectStorageChanged(ICimDataModel? sender,
        IModelObject modelObject, CimDataModelObjectStorageChangedEventArgs e)
    {
        switch (e.ChangeType)
        {
            case CimDataModelObjectStorageChangeType.Remove:
            {
                var row = OidToRow(modelObject.OID);
                if (row == null) return;

                RemoveObjectRow(row);

                break;
            }
            case CimDataModelObjectStorageChangeType.Add:
            {
                CreateObjectRow(modelObject);

                break;
            }
        }

        OnPropertyChanged(nameof(CimObjectsSource));
    }

    private void RemoveObjectRow(CimObjectDataTreeModel row)
    {
        if (row.ParentNode == null)
        {
            _nodesCache.Remove(row);
            return;
        }

        var root = row.ParentNode;
        while (root.ParentNode != null) root = root.ParentNode;

        if (root is not TreeViewNodeModel rootModel) throw new Exception("Root node is not a TreeViewNodeModel");

        var id = _nodesCache.IndexOf(rootModel);
        _nodesCache.RemoveAt(id);
        row.ParentNode = null;
        _nodesCache.Insert(id, rootModel);

        if (rootModel.SubNodes.Count == 0) _nodesCache.Remove(rootModel);
    }

    private void CreateObjectRow(IModelObject modelObject)
    {
        var classUri = modelObject.MetaClass.BaseUri.AbsoluteUri;
        var findClassNode = _nodesCache.FirstOrDefault(cn => cn.Title == classUri);

        if (findClassNode == null)
        {
            findClassNode = new TreeViewNodeModel { Title = classUri };
            _nodesCache.Add(findClassNode);
        }

        var id = _nodesCache.IndexOf(findClassNode);
        _nodesCache.RemoveAt(id);
        findClassNode.AddChild(
            new CimObjectDataTreeModel(modelObject));
        _nodesCache.Insert(id, findClassNode);

        Find(modelObject.OID.ToString());
    }

    private CimObjectDataTreeModel? OidToRow(IOIDDescriptor descriptor)
    {
        foreach (var node in _nodesCache)
        {
            var deepSearch = new Stack<LinkedNodeModel>(node.SubNodes);
            deepSearch.Push(node);

            while (deepSearch.TryPop(out var subNode))
            {
                if (subNode is not CimObjectDataTreeModel cimNode) continue;

                if (cimNode.ModelObject.OID == descriptor) return cimNode;
            }
        }

        return null;
    }

    private void LoadObjectsCache()
    {
        _nodesCache.Clear();

        if (CimModelDocument == null) return;

        var schemaClassesUri =
            new Dictionary<Uri, TreeViewNodeModel>(new RdfUriComparer());

        foreach (var cimObj in CimModelDocument.GetAllObjects())
        {
            var cimObjNode = new CimObjectDataTreeModel(cimObj);

            var classUri = cimObj.MetaClass.BaseUri;
            if (schemaClassesUri.TryGetValue(classUri, out var classNode))
            {
                classNode.AddChild(cimObjNode);
            }
            else
            {
                var newClassNode = new TreeViewNodeModel
                    { Title = classUri.AbsoluteUri };

                newClassNode.AddChild(cimObjNode);

                _nodesCache.Add(newClassNode);
                schemaClassesUri.Add(classUri, newClassNode);
            }
        }
    }
}