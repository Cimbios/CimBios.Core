using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;
using CimBios.Tools.ModelDebug.Models;
using CimBios.Tools.ModelDebug.Services;
using CimBios.Tools.ModelDebug.Views;

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

    public async Task EditPropertyValue()
    {
        var result = await GlobalServices.DialogService
            .ShowDialog<CimPropertyValueEditorDialog>();

        if (result is not CimPropertyValueEditorDialogResult openSaveResult
            || !openSaveResult.Succeed) return;
    }

    private void SubscribeToNavigationService()
    {
        GlobalServices.NavigationService.OnSelectionChanged +=
            (s, e) => OnObjectSelectionChanged(
                (e as CimObjectSelectionChangedArgs)?.ModelObject);
    }

    private void OnObjectSelectionChanged(IModelObject? selectedObject)
    {
        _propCache.Clear();
        SelectedUuid = string.Empty;
        
        OnPropertyChanged(nameof(PropertySource));

        if (selectedObject == null)
        {
            return;
        }

        SelectedUuid = selectedObject.OID.ToString();
        ShowSelectedObjectProperties(selectedObject);
    }

    private void ShowSelectedObjectProperties(IModelObject selectedObject, 
        TreeViewNodeModel? parent=null)
    {
        var sortedProps =  selectedObject.MetaClass
            .AllProperties.OrderBy(p => p.PropertyKind)
                .ThenBy(p => p.ShortName);

        foreach (var prop in sortedProps)
        {
            var propName = $"{prop.OwnerClass?.ShortName}.{prop.ShortName}";
            
            var propNode = new CimObjectPropertyModel() 
                { Name = propName, Value = "null" };

            if (prop.PropertyKind == CimMetaPropertyKind.Attribute)
            {
                var propValueObject = selectedObject.GetAttribute<object>(prop);
                if (propValueObject is IModelObject compound)
                {
                    propNode.Value = compound.MetaClass.ShortName;
                    ShowSelectedObjectProperties(compound, propNode);
                }
                else
                {
                    propNode.Value = selectedObject.GetAttribute<object>(prop)?
                        .ToString() ?? "null";
                }
            }
            else if (prop.PropertyKind == CimMetaPropertyKind.Assoc1To1)
            {
                var propValueRef1 = selectedObject.GetAssoc1To1(prop);
                if (propValueRef1 != null)
                {
                    propNode.Value = propValueRef1.OID.ToString();
                }
            }
            else if (prop.PropertyKind == CimMetaPropertyKind.Assoc1ToM)
            {
                var propValueRefM = selectedObject.GetAssoc1ToM(prop);
                propNode.Value = $"Count: {propValueRefM.Length}";
                foreach (var propRef in propValueRefM)
                {
                    var refMNode = new CimObjectPropertyModel()
                        { Name = String.Empty, Value = propRef.OID.ToString() };

                    propNode.AddChild(refMNode);
                }
            }
            else if (prop.PropertyKind == CimMetaPropertyKind.Statements)
            {
                if (selectedObject is IStatementsContainer statementsContainer)
                {
                    propNode.Value = $"Statements: {statementsContainer.Statements.Count}";
                    
                    foreach (var statementObject in statementsContainer.Statements[prop])
                    {
                        var statementNode = new CimObjectPropertyModel()
                            { Name = "Statement", Value = statementObject.MetaClass.ShortName };
                            
                        ShowSelectedObjectProperties(statementObject, statementNode);

                        propNode.AddChild(statementNode);
                    }
                }
            }
            else
            {
                propNode.Value = "<Not supported>";
            }

            if (parent == null)
            {
                _propCache.Add(propNode);
            }
            else
            {
                parent.AddChild(propNode);
            }
        }
    }

    private readonly ObservableCollection<CimObjectPropertyModel> _propCache = [];

    private string _selectedUuid = string.Empty;
}
