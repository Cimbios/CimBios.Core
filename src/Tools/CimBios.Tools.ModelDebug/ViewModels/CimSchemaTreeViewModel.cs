using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CimBios.Core.CimModel.Context;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.RdfXmlIOLib;
using CimBios.Tools.ModelDebug.Models;

namespace CimBios.Tools.ModelDebug.ViewModels;

public class CimSchemaTreeViewModel : ViewModelBase, INotifyPropertyChanged
{
    public ObservableCollection<TreeViewNodeModel> Nodes { get; } 
    public TreeViewNodeModel? SelectedItem { 
        get => _SelectedItem; 
        set
        {
            _SelectedItem = value; 
            OnPropertyChanged(nameof(SelectedItem));   
        }
    }

    private ICimSchema? CimSchemaContext { get; set; }

    public CimSchemaTreeViewModel()
    {
        Nodes = new ObservableCollection<TreeViewNodeModel>();

        SubscribeModelContextLoad();
    }

    public Task ExpandTree(object? sender)
    {
        if (sender is TreeView senderTreeView == false)
        {
            return Task.CompletedTask;
        }

        return DoExpandAllNodes(senderTreeView, true);
    }

    public Task UnexpandTree(object? sender)
    {
        if (sender is TreeView senderTreeView == false)
        {
            return Task.CompletedTask;
        }

        return DoExpandAllNodes(senderTreeView, false);
    }

    public Task DoExpandAllNodes(TreeView senderTreeView, bool IsExpand)
    {
        foreach (var node in Nodes)
        {
            var treeViewItemContainer = senderTreeView.ContainerFromItem(node);
            if (treeViewItemContainer is TreeViewItem treeViewItem)
            {
                treeViewItem.IsExpanded = IsExpand;
            }
        }

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

        CimSchemaContext = modelContext.Schema;
        FillData();
    }

    private void FillData()
    {
        Nodes.Clear();

        if (CimSchemaContext == null)
        {
            return;
        }

        HandleClassNodes();
    }

    private void HandleClassNodes()
    {
        var uriVsNode = new Dictionary<Uri, CimSchemaEntityNodeModel>(
            new RdfUriComparer());

        foreach (var schemaClass in CimSchemaContext!.Classes)
        {
            if (uriVsNode.ContainsKey(schemaClass.BaseUri))
            {
                continue;
            }

            var classPrefix = CimSchemaContext.
                GetUriNamespacePrefix(schemaClass.BaseUri);

            var classTraits = GetClassTraitsString(schemaClass);

            var node = new CimSchemaEntityNodeModel(schemaClass)
            {
                Title = $"[C] {classPrefix}:{schemaClass.ShortName} {classTraits}"
            };
            uriVsNode.Add(schemaClass.BaseUri, node);

            HandleClassPropertiesNodes(node, 
                CimSchemaContext.GetClassProperties(schemaClass));

            HandleClassIndividualsNodes(node, 
                CimSchemaContext.GetClassIndividuals(schemaClass));
        }

        foreach (var schemaClass in CimSchemaContext.Classes)
        {
            if (uriVsNode.TryGetValue(schemaClass.BaseUri, 
                out var classNode))
            {
                if (schemaClass.ParentClass == null)
                {
                    Nodes.Add(classNode);

                    if (SelectedItem == null && classNode.SubNodes.Count != 0)
                    {
                        SelectedItem = classNode.SubNodes.FirstOrDefault();
                    }        
                }
                else
                {
                    if (uriVsNode.TryGetValue(schemaClass.ParentClass.BaseUri, 
                        out var parentNode))
                    {
                        parentNode.SubNodes.Add(classNode);
                    }
                }
            }
        }  
    }

    private void HandleClassPropertiesNodes(CimSchemaEntityNodeModel classNode,
        IEnumerable<ICimMetaProperty> properties)
    {
        foreach (var prop in properties)
        {
            var propPrefix = CimSchemaContext!
                .GetUriNamespacePrefix(prop.BaseUri);

            string propKind = GetPropertyKindString(prop);

            var dataType = GetPropertyType(prop);

            var propNode = new CimSchemaEntityNodeModel(prop)
            {
                Title = $"{propKind} {propPrefix}:{prop.ShortName} {dataType}"
            };

            classNode.SubNodes.Add(propNode);
        }
    }

    private void HandleClassIndividualsNodes(CimSchemaEntityNodeModel classNode,
        IEnumerable<ICimMetaInstance> individuals)
    {
        foreach (var individual in individuals)
        {
            var individualrefix = CimSchemaContext!
                .GetUriNamespacePrefix(individual.BaseUri);

            var individualNode = new CimSchemaEntityNodeModel(individual)
            {
                Title = $"[I] {individualrefix}:{individual.ShortName}"
            };
            classNode.SubNodes.Add(individualNode);
        }
    }

    private string GetClassTraitsString(ICimMetaClass cimMetaClass)
    {
        if (cimMetaClass.IsCompound && cimMetaClass.IsEnum)
        {
            return "(Compound, Enum)";
        }
        else if (cimMetaClass.IsCompound)
        {
            return "(Compound)";
        }
        else if (cimMetaClass.IsEnum)
        {
            return "(Enum)";
        }
        else
        {
            return string.Empty;
        }
    }

    private string GetPropertyKindString(ICimMetaProperty property)
    {
        if (property.PropertyKind == CimMetaPropertyKind.Attribute)
        {
            return "[Attr]";
        }
        else if (property.PropertyKind == CimMetaPropertyKind.Assoc1To1)
        {
            return "[Assoc1To1]";
        }
        else if (property.PropertyKind == CimMetaPropertyKind.Assoc1ToM)
        {
            return "[Assoc1ToM]";
        }       

        return "[P]";
    }

    private string GetPropertyType(ICimMetaProperty property)
    {
        if (property.PropertyDatatype is ICimMetaDatatype datatype)
        {
            if (datatype.ShortName == string.Empty)
            {
                return $"<{datatype.SimpleType.Name}>"; 
            }

            return $"<{datatype.ShortName}/{datatype.SimpleType.Name}>";
        }

        if (property.PropertyDatatype is ICimMetaClass metaClass)
        {
            return $"<{metaClass.ShortName}>";
        }

        return string.Empty;
    }

    private TreeViewNodeModel? _SelectedItem;
}
