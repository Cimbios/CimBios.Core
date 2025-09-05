using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.Schema;
using CimBios.Tools.ModelDebug.Models;
using CimBios.Tools.ModelDebug.Services;
using CimBios.Tools.ModelDebug.Views;

namespace CimBios.Tools.ModelDebug.ViewModels;

public class CimObjectPropertiesObserverViewModel : ViewModelBase
{
    public HierarchicalTreeDataGridSource<CimObjectPropertyModel> PropertySource 
    { get; }

    public IModelObject? SelectedObject
    {
        get => _selectedObject;
        private set
        {
            _selectedObject = value;
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
                        ("Name", x 
                            => $"{x.MetaProperty.OwnerClass!.ShortName}.{x.MetaProperty.ShortName}"),
                    x => x.SubNodes.Cast<CimObjectPropertyModel>(), 
                    x => x.SubNodes.Count != 0),
                new TextColumn<CimObjectPropertyModel, string>
                    ("Value", x => x.Value),
            },
        };

        PropertySource.RowSelection!.SingleSelect = true;

        SubscribeToNavigationService();
    }

    public void ClearPropertyValue()
    {
        if (PropertySource.RowSelection!.SelectedItem 
            is not { } selectedProp)
        {
            return;
        }

        if (selectedProp.MetaProperty.PropertyKind
            == CimMetaPropertyKind.Attribute)
        {
            selectedProp.ModelObject.SetAttribute<object>(
                selectedProp.MetaProperty, null);
        }
        else if (selectedProp.MetaProperty.PropertyKind 
                 == CimMetaPropertyKind.Assoc1To1)
        {
            selectedProp.ModelObject.SetAssoc1To1(
                selectedProp.MetaProperty, null);
        }
        else if (selectedProp.MetaProperty.PropertyKind 
                 == CimMetaPropertyKind.Assoc1ToM)
        {
            selectedProp.ModelObject.RemoveAllAssocs1ToM(
                selectedProp.MetaProperty);
        }
        
        _propCache.Clear();
        ShowSelectedObjectProperties(SelectedObject!);
    }

    public void SetDefaultPropertyValue()
    {
        if (PropertySource.RowSelection!.SelectedItem 
            is not { } selectedProp)
        {
            return;
        }
        
        if (selectedProp.MetaProperty.PropertyKind 
            == CimMetaPropertyKind.Attribute)
        {
            if (selectedProp.MetaProperty.PropertyDatatype?.IsCompound ?? false)
            {
                selectedProp.ModelObject.InitializeCompoundAttribute(
                    selectedProp.MetaProperty);

                return;
            }

            if (selectedProp.MetaProperty.PropertyDatatype is not null)
            {
                object? defaultValue = null;
                
                if (selectedProp.MetaProperty.PropertyDatatype
                    is ICimMetaDatatype datatype)
                {
                    defaultValue = GetDefaultValue(datatype.PrimitiveType);
                }
                else if (selectedProp.MetaProperty.PropertyDatatype.IsEnum)
                {
                    var defaultEnum = selectedProp.MetaProperty
                        .PropertyDatatype.AllIndividuals.FirstOrDefault();

                    if (defaultEnum is not null)
                    {
                        selectedProp.ModelObject.SetAttributeAsEnum(
                            selectedProp.MetaProperty, defaultEnum);
                        return;
                    }
                }
                
                selectedProp.ModelObject.SetAttribute<object>(
                    selectedProp.MetaProperty, defaultValue);
            }
        }
        
        _propCache.Clear();
        ShowSelectedObjectProperties(SelectedObject!);
    }
    
    private static object? GetDefaultValue(Type t)
    {
        if (t == typeof(string))
        {
            return string.Empty;
        }
        
        return t.IsValueType ? Activator.CreateInstance(t) : null;
    }

    public void Navigate()
    {
        IModelObject? refObject = null;
        if (PropertySource.RowSelection!.SelectedItem 
            is CimAssocPropertyModel assocProp)
        {
            refObject = assocProp.AssocObject;
        }
        else if (PropertySource.RowSelection!.SelectedItem 
                 is { } selectedProp)
        {
            refObject = selectedProp.ModelObject
                .GetAssoc1To1(selectedProp.MetaProperty);
        }
        
        if (refObject == null) return;
        
        GlobalServices.NavigationService
            .Select(refObject.OID.ToString());
    }

    public async Task EditPropertyValue()
    {
        var dataContext = GlobalServices.LoaderService.DataContext;
        if (dataContext == null) return;
        
        if (PropertySource.RowSelection!.SelectedItem 
            is not { } selectedProp)
        {
            return;
        }

        if (selectedProp.MetaProperty.PropertyKind != CimMetaPropertyKind.Assoc1To1
            && selectedProp.MetaProperty.PropertyKind != CimMetaPropertyKind.Assoc1ToM
            && selectedProp.MetaProperty.PropertyKind != CimMetaPropertyKind.Attribute)
        {
            return;
        }

        if (selectedProp.MetaProperty.PropertyDatatype?.IsCompound ?? true)
        {
            return;
        }
        
        var result = await GlobalServices.DialogService
            .ShowDialog<CimPropertyValueEditorDialog>(selectedProp);

        if (result is not CimPropertyValueEditorDialogResult openSaveResult
            || !openSaveResult.Succeed) return;

        try
        {
            if (selectedProp.MetaProperty is
                {
                    PropertyKind: CimMetaPropertyKind.Attribute, 
                    PropertyDatatype: ICimMetaDatatype datatype,
                    PropertyDatatype.IsEnum: false
                })
            {
                EditPrimitiveAttribute(openSaveResult, datatype, selectedProp);
            }

            if (selectedProp.MetaProperty is
                {
                    PropertyKind: CimMetaPropertyKind.Attribute,
                    PropertyDatatype.IsEnum: true
                })
            {
                EditEnumAttribute(selectedProp, openSaveResult);
            }

            if (selectedProp.MetaProperty is
                {
                    PropertyKind: CimMetaPropertyKind.Assoc1To1,
                })
            {
                var oid11 = dataContext.OIDDescriptorFactory
                    .Create(openSaveResult.Value.Trim());
                var refObj = dataContext.GetObject(oid11);
                if (refObj == null) return;
                
                selectedProp.ModelObject
                    .SetAssoc1To1(selectedProp.MetaProperty, refObj);
            }
            
            if (selectedProp.MetaProperty is
                {
                    PropertyKind: CimMetaPropertyKind.Assoc1ToM,
                })
            {
                var oids1M = openSaveResult.Value
                    .Trim().Split(';')
                    .Select(s => 
                        dataContext.OIDDescriptorFactory.Create(s.Trim()))
                    .ToArray();

                var currentOidsM = selectedProp.ModelObject
                    .GetAssoc1ToM(selectedProp.MetaProperty)
                    .Select(o => o.OID).ToArray();
                
                var forRemoveOids = currentOidsM.Except(oids1M);
                var forAddOids = oids1M.Except(currentOidsM);
                
                foreach (var oid in forRemoveOids)
                    selectedProp.ModelObject.RemoveAssoc1ToM(
                        selectedProp.MetaProperty, dataContext.GetObject(oid)
                        ?? throw new NullReferenceException());
                
                foreach (var oid in forAddOids)
                    selectedProp.ModelObject.AddAssoc1ToM(
                        selectedProp.MetaProperty, dataContext.GetObject(oid)
                        ?? throw new NullReferenceException());
            }
        }
        catch (Exception e)
        {
            GlobalServices.ProtocolService
                .Error(e.Message, 
                    "CimObjectPropertiesObserverViewModel.EditPropertyValue");
        }
    }

    private static void EditEnumAttribute(CimObjectPropertyModel selectedProp,
        CimPropertyValueEditorDialogResult openSaveResult)
    {
        var cimMetaIndividual = selectedProp.MetaProperty.PropertyDatatype?
            .AllIndividuals.FirstOrDefault(
                i => i.ShortName == openSaveResult.Value);

        if (cimMetaIndividual != null)
        {
            selectedProp.ModelObject.SetAttributeAsEnum(
                selectedProp.MetaProperty, cimMetaIndividual);
        }
    }

    private static void EditPrimitiveAttribute(
        CimPropertyValueEditorDialogResult openSaveResult, 
        ICimMetaDatatype datatype,
        CimObjectPropertyModel selectedProp)
    {
        var convertedValue = Convert.ChangeType(openSaveResult.Value,
            datatype.PrimitiveType, CultureInfo.InvariantCulture);

        selectedProp.ModelObject.SetAttribute(
            selectedProp.MetaProperty, convertedValue);
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
        SelectedObject = null;
        
        OnPropertyChanged(nameof(PropertySource));

        if (selectedObject == null)
        {
            return;
        }

        SelectedObject = selectedObject;
        ShowSelectedObjectProperties(selectedObject);
    }

    private void ShowSelectedObjectProperties(IModelObject selectedObject)
    {
        var sortedProps =  selectedObject.MetaClass
            .AllProperties.OrderBy(p => p.PropertyKind)
                .ThenBy(p => p.ShortName);

        foreach (var prop in sortedProps)
        {
            var propNode = new CimObjectPropertyModel(selectedObject, prop, true);
            _propCache.Add(propNode);
        }
    }

    private readonly ObservableCollection<CimObjectPropertyModel> _propCache = [];
    
    private IModelObject? _selectedObject = null;
}
