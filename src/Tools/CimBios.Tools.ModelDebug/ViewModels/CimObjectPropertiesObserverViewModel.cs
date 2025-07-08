using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;
using CimBios.Tools.ModelDebug.Models;
using CimBios.Tools.ModelDebug.Services;

namespace CimBios.Tools.ModelDebug.ViewModels;

public class CimObjectPropertiesObserverViewModel : ViewModelBase
{
    public HierarchicalTreeDataGridSource<CimObjectPropertyModel> PropertySource 
    { get; }

    public string SelectedUuid 
    { 
        get => _selectedUuid; 
        set
        {
            _selectedUuid = value;
            OnPropertyChanged();
        }
    }

    public CimObjectPropertiesObserverViewModel()
    {
        PropertySource = new HierarchicalTreeDataGridSource
            <CimObjectPropertyModel>(_propCache)
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

        SubscribeToNavigationService();
    }

    public void Navigate()
    {
        if (PropertySource.RowSelection!.SelectedItem 
            is not { } selectedProp)
        {
            return;
        }

        GlobalServices.NavigationService.Select(selectedProp.Value);
    }

    private void SubscribeToNavigationService()
    {
        GlobalServices.NavigationService.OnSelectionChanged += 
            (s, e) => ShowSelectedProperties(
                (e as CimObjectSelectionChangedArgs)?.ModelObject);
    }

    private void ShowSelectedProperties(IModelObject? selectedObject)
    {
        _propCache.Clear();
        SelectedUuid = string.Empty;
        
        OnPropertyChanged(nameof(PropertySource));

        if (selectedObject == null)
        {
            return;
        }

        SelectedUuid = selectedObject.OID.ToString();

        foreach (var attrName in selectedObject.MetaClass.AllProperties
            .Where(p => p.PropertyKind == CimMetaPropertyKind.Attribute)
            .Select(p => p.ShortName))
        {
            var attrValue = selectedObject.GetAttribute<object>(attrName);
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
                 foreach (var compoundAttrName in compoundAttr.MetaClass.AllProperties
                    .Where(p => p.PropertyKind == CimMetaPropertyKind.Attribute)
                    .Select(p => p.ShortName))
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

            _propCache.Add(attrNode);
        }

        foreach (var assoc11Name in selectedObject.MetaClass.AllProperties
            .Where(p => p.PropertyKind == CimMetaPropertyKind.Assoc1To1)
            .Select(p => p.ShortName))
        {
            var assoc11Ref = selectedObject.GetAssoc1To1<IModelObject>(assoc11Name);
            string assoc11RefStr = "null";
            if (assoc11Ref != null)
            {
                assoc11RefStr = assoc11Ref.OID.ToString();
            }

            _propCache.Add(new CimObjectPropertyModel() 
                { Name = assoc11Name, Value = assoc11RefStr });
        }
        
        foreach (var assoc1MName in selectedObject.MetaClass.AllProperties
            .Where(p => p.PropertyKind == CimMetaPropertyKind.Assoc1ToM)
            .Select(p => p.ShortName))
        {
            var assoc1MArray = selectedObject
                .GetAssoc1ToM(assoc1MName);

            var assoc1MNode = new CimObjectPropertyModel() 
                { Name = assoc1MName, Value = $"Count: {assoc1MArray.Count()}" };

            foreach (var assoc1MRef in assoc1MArray.OfType<IModelObject>())
            {
                assoc1MNode.AddChild(new CimObjectPropertyModel() 
                { Name = string.Empty, Value = assoc1MRef.OID.ToString() });
            }

            _propCache.Add(assoc1MNode);
        }    
    }

    private ObservableCollection<CimObjectPropertyModel> _propCache = [];

    private string _selectedUuid = string.Empty;
}
