using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Tools.ModelDebug.Models;
using CimBios.Tools.ModelDebug.Models.CimObjects;

namespace CimBios.Tools.ModelDebug.ViewModels;

public class PropertyDiffsObserverViewModel : TreeViewNodeModel
{
    public HierarchicalTreeDataGridSource<DiffObjectPropertyModel> TreeSource { get; }
    private readonly ObservableCollection<DiffObjectPropertyModel> _cache = [];

    public PropertyDiffsObserverViewModel()
    {
        TreeSource = new HierarchicalTreeDataGridSource<DiffObjectPropertyModel>(_cache);
        TreeSource.Columns.AddRange(MakeModelColumnList());
    }
    
    private static ColumnList<DiffObjectPropertyModel> MakeModelColumnList()
    {
        return
        [
            new HierarchicalExpanderColumn<DiffObjectPropertyModel>(
                new TextColumn<DiffObjectPropertyModel, string>("Property",
                    x => $"{x.MetaProperty.OwnerClass.ShortName}.{x.MetaProperty.ShortName}"),
                x => x.SubNodes.Cast<DiffObjectPropertyModel>(),
                x => x.SubNodes.Count != 0,
                x => x.IsExpanded),
            
            new TextColumn<DiffObjectPropertyModel, string>("Old",
                x => $"{x.OldValue}"),
            
            new TextColumn<DiffObjectPropertyModel, string>("New",
                x => $"{x.NewValue}")
        ];
    }
    
    public void InitializeData(IDifferenceObject difference)
    {
        Clear();

        foreach (var metaProperty in difference.ModifiedProperties)
        {
            _cache.Add(new DiffObjectPropertyModel(difference, metaProperty));
        }
    }

    public void Clear()
    {
        _cache.Clear();
    }
}