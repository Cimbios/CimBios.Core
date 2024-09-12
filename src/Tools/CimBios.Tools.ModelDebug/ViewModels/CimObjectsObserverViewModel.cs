using System;
using System.ComponentModel;
using Avalonia.Controls;
using CimBios.Tools.ModelDebug.Models;

namespace CimBios.Tools.ModelDebug.ViewModels;

public class CimObjectsObserverViewModel : ViewModelBase
{
    public HierarchicalTreeDataGridSource<CimObjectDataTreeModel> CimObjectsSource { get; }

    
}
