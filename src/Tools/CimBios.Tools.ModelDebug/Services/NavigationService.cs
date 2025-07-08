using System;
using System.Linq;
using Avalonia.Controls.Selection;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Tools.ModelDebug.Models;
using CimBios.Tools.ModelDebug.ViewModels;

namespace CimBios.Tools.ModelDebug.Services;

public class NavigationService
{
    public NavigationService(CimObjectsObserverViewModel objectsObserver)
    {
        _ObjectsObserver = objectsObserver;

        _ObjectsObserver.CimObjectsSource.RowSelection!.SelectionChanged
            += CellSelection_SelectionChanged;
    }

    public IModelObject? SelectedObject { get; private set; }

    private CimObjectsObserverViewModel _ObjectsObserver { get; }

    public event EventHandler? OnSelectionChanged;

    public void Select(string uuid)
    {
        _ObjectsObserver.Find(uuid);
    }

    private void CellSelection_SelectionChanged(object? sender,
        TreeSelectionModelSelectionChangedEventArgs<TreeViewNodeModel> e)
    {
        var selectedItem = e.SelectedItems.FirstOrDefault();

        if (selectedItem is not CimObjectDataTreeModel cimObjectItem)
        {
            OnSelectionChanged?.Invoke(this,
                new CimObjectSelectionChangedArgs(null));

            return;
        }

        SelectedObject = cimObjectItem.ModelObject;

        OnSelectionChanged?.Invoke(this,
            new CimObjectSelectionChangedArgs(SelectedObject));
    }
}

public class CimObjectSelectionChangedArgs(IModelObject? modelObject)
    : EventArgs
{
    public IModelObject? ModelObject { get; } = modelObject;
}