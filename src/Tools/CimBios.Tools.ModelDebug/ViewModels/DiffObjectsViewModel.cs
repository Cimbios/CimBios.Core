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
using CimBios.Core.CimModel.CimDifferenceModel;
using CimBios.Core.CimModel.DataModel.Utils;
using CimBios.Tools.ModelDebug.Models.CimObjects;
using CimBios.Tools.ModelDebug.Views;

namespace CimBios.Tools.ModelDebug.ViewModels;

public class DiffObjectsViewModel : ViewModelBase
{
    public HierarchicalTreeDataGridSource<DiffObjectModel> TreeSource { get; }

    public DiffObjectModel? SelectedObject { get; private set; } = null;
    
    private TreeDataGrid DataGridControl { get; }
    
    private PropertyDiffsObserverControl PropertyDiffsObserver { get; }

    private readonly ObservableCollection<DiffObjectModel> _diffsCache = [];
    private ICimDifferenceModel? _currentModel = null;
    
    public DiffObjectsViewModel(TreeDataGrid dataGridControl, 
        PropertyDiffsObserverControl propertyDiffsObserverViewModel)
    {
        DataGridControl = dataGridControl;
        PropertyDiffsObserver = propertyDiffsObserverViewModel;

        TreeSource = new HierarchicalTreeDataGridSource<DiffObjectModel>(_diffsCache);
        TreeSource.Columns.AddRange(MakeModelColumnList());

        ConfigureSourceSelection(TreeSource);
    }

    public void ApplyDiffsToModel()
    {
        if (GlobalServices.LoaderService.DataContext == null) return;
        if (_currentModel == null) return;

        try
        {
            GlobalServices.LoaderService.DataContext
                .ApplyDifferenceModel(_currentModel);
        }
        catch (Exception e)
        {
            GlobalServices.ProtocolService
                .Error($"Apply diffs failed: {e.Message}", "Diffs");
        }
    }

    public async Task LoadDifferenceModelFromFile()
    {
        var result = await GlobalServices.DialogService
            .ShowDialog<CimModelOpenSaveWindow>(
                CimModelOpenSaveWindow.DialogMode.Load);

        if (result is not CimModelOpenSaveResult openSaveResult
            || !openSaveResult.Succeed) return;

        var diffModel = GlobalServices.LoaderService.LoadDifferenceModelFromFile(
            openSaveResult.ModelPath,
            openSaveResult.DescriptorFactory,
            openSaveResult.RdfSerializerFactory,
            out _
        );
        
        if (diffModel is null) return;
        
        InitializeData(diffModel);
    }

    public void SwitchDataToLocalDiffs()
    {
        var localDiffs = GlobalServices.LoaderService.LocalDifferences;
        if (localDiffs == null) return;
        
        InitializeData(localDiffs);
    }

    public async Task SaveDiffs()
    {
        if (_currentModel == null) return;
        
        var result = await GlobalServices.DialogService
            .ShowDialog<CimModelOpenSaveWindow>(
                CimModelOpenSaveWindow.DialogMode.Save);

        if (result is not CimModelOpenSaveResult openSaveResult
            || !openSaveResult.Succeed) return;

        GlobalServices.LoaderService.SaveDifferenceModelToFile(
            _currentModel,
            openSaveResult.ModelPath,
            openSaveResult.RdfSerializerFactory,
            out _
        );
    }

    public async Task CompareModels()
    {
        var dataContext = GlobalServices.LoaderService.DataContext;
        if (dataContext == null) return;
        
        var result = await GlobalServices.DialogService
            .ShowDialog<CimModelOpenSaveWindow>(
                CimModelOpenSaveWindow.DialogMode.Load);

        if (result is not CimModelOpenSaveResult openSaveResult
            || !openSaveResult.Succeed) return;

        var comparedModel = GlobalServices.LoaderService
            .CompareDataContextWith(
                openSaveResult.ModelPath, 
                openSaveResult.SchemaPath, 
                openSaveResult.DescriptorFactory, 
                openSaveResult.SchemaFactory, 
                openSaveResult.RdfSerializerFactory,
                openSaveResult.SerializerSettings, out _);
        
        if (comparedModel is null) return;
        
        InitializeData(comparedModel);
    }
    
    public void InitializeData(ICimDifferenceModel differenceModel)
    {
        _diffsCache.Clear();
        if (_currentModel != null)
            _currentModel.DifferencesStorageChanged
                += OnDifferencesStorageChanged;
        _currentModel = null;

        List<Type> typeSortOrder = [
            typeof(AdditionDifferenceObject), 
            typeof(DeletionDifferenceObject), 
            typeof(UpdatingDifferenceObject)];
        
        foreach (var diff in differenceModel.Differences
                     .OrderBy(o => typeSortOrder.IndexOf(o.GetType()))
                     .ThenBy(o => o.MetaClass.ShortName))
            _diffsCache.Add(new DiffObjectModel(diff));

        differenceModel.DifferencesStorageChanged 
            += OnDifferencesStorageChanged;
        _currentModel = differenceModel;
    }

    private void OnDifferencesStorageChanged(
        ICimDataModel? sender, 
        IDifferenceObject differenceObject, 
        CimDataModelObjectStorageChangedEventArgs e)
    {
        switch (e.ChangeType)
        {
            case CimDataModelObjectStorageChangeType.Add:
                _diffsCache.Add(new DiffObjectModel(differenceObject));
                break;
            case CimDataModelObjectStorageChangeType.Remove:
            {
                var model = ObjectToModel(differenceObject);
                if (model != null) _diffsCache.Remove(model);
                break;
            }
        }
    }

    private DiffObjectModel? ObjectToModel(IDifferenceObject differenceObject)
    {
        return _diffsCache.FirstOrDefault(
            d => 
                d.DifferenceObject.OID == differenceObject.OID);
    }

    private static ColumnList<DiffObjectModel> MakeModelColumnList()
    {
        return
        [
            new HierarchicalExpanderColumn<DiffObjectModel>(
                new TextColumn<DiffObjectModel, string>("Type",
                    x => $"{x.Type}"),
                x => x.SubNodes.Cast<DiffObjectModel>(),
                x => x.SubNodes.Count != 0,
                x => x.IsExpanded),
            
            new TextColumn<DiffObjectModel, string>("OID",
                x => $"{x.DifferenceObject.OID}"),
            
            new TextColumn<DiffObjectModel, string>("Class",
                x => GetClassNameOfDifferenceObject(x.DifferenceObject))
        ];
    }

    private void ConfigureSourceSelection<T>(
        HierarchicalTreeDataGridSource<T> source) where T : class
    {
        if (source.RowSelection == null) return;
        
        source.RowSelection.SingleSelect = true;
        source.RowSelection.SelectionChanged += CellSelectionOnSelectionChanged;
    }

    private void CellSelectionOnSelectionChanged(object? sender,
        TreeSelectionModelSelectionChangedEventArgs e)
    {
        SelectedObject = null;
        
        var selectedItem = e.SelectedItems.FirstOrDefault();

        if (selectedItem is not DiffObjectModel doModel) return;
        
        SelectedObject = doModel;

        PropertyDiffsObserver.SelectedDiff = SelectedObject.DifferenceObject;
    }
    
    private static string GetClassNameOfDifferenceObject(
        IDifferenceObject differenceObject)
    {
        var model = GlobalServices.LoaderService.DataContext;
        if (model?.GetObject(differenceObject.OID) is { } modelObject)
        {
            return modelObject.MetaClass.BaseUri.AbsoluteUri;
        }
                    
        return differenceObject.MetaClass.BaseUri.AbsoluteUri;
    }
}