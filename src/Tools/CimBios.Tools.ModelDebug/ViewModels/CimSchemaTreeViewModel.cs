using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using CimBios.Core.CimModel.Context;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.RdfIOLib;
using CimBios.Tools.ModelDebug.Models;
using CommunityToolkit.Mvvm.Input;

namespace CimBios.Tools.ModelDebug.ViewModels;

public class CimSchemaTreeViewModel : TreeViewModelBase
{
    public override IEnumerable<CimSchemaEntityNodeModel> Nodes
    { 
        get
        {
            return _NodesCache;
        }
    }

    public string SearchString 
    { 
        get => _SearchString; 
        set
        {
            _SearchString = value;
            OnPropertyChanged(nameof(SearchString));

            ApplyFilter(FilterNode);
        }
    } 

    public bool ShowProperties 
    { 
        get => _ShowProperties; 
        set
        {
            _ShowProperties = value;
            OnPropertyChanged(nameof(ShowProperties));

            ApplyFilter(FilterNode);
        }
    }

    public bool ShowIndividuals 
    { 
        get => _ShowIndividuals; 
        set
        {
            _ShowIndividuals = value;
            OnPropertyChanged(nameof(ShowIndividuals));

            ApplyFilter(FilterNode);
        }
    }

    public AsyncRelayCommand ExpandAllNodesCommand { get; }
    public AsyncRelayCommand UnexpandAllNodesCommand { get; }

    private ICimSchema? CimSchemaContext { get; set; }

    public CimSchemaTreeViewModel()
    {
        _NodesCache = new ObservableCollection<CimSchemaEntityNodeModel>();
        _NodesCache.CollectionChanged += NodesCache_CollectionChanged;

        ExpandAllNodesCommand = new AsyncRelayCommand
            (() => DoExpandAllNodes(true));

        UnexpandAllNodesCommand = new AsyncRelayCommand
            (() => DoExpandAllNodes(false));

        SubscribeModelContextLoad();
    }

    private bool FilterNode(TreeViewNodeModel node)
    {
        if (node is CimSchemaEntityNodeModel schemaNode == false)
        {
            node.IsExpanded = true;
            return true;
        }

        if ((schemaNode.CimSchemaEntity is ICimMetaProperty
                && ShowProperties == false)
            || (schemaNode.CimSchemaEntity is ICimMetaIndividual)
                && ShowIndividuals == false)
        {
            return false;
        }

        if (SearchString.Trim() == string.Empty)
        {
            DoExpandAllNodes(false);
            return true;
        }

        if (schemaNode.Title.Contains(SearchString))
        {
            return true;
        }

        return false;
    }

    private void NodesCache_CollectionChanged(object? sender, 
        NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(Nodes));
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
        _NodesCache.Clear();

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
                Title = $"[C] {classPrefix}:{schemaClass.ShortName} ({classTraits})"
            };
            uriVsNode.Add(schemaClass.BaseUri, node);

            HandleClassPropertiesNodes(node, schemaClass.AllProperties);

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
                    _NodesCache.Add(classNode);

                    if (SelectedItem == null && classNode.SubNodes.Count() != 0)
                    {
                        SelectedItem = classNode.SubNodes.FirstOrDefault() 
                            as TreeViewNodeModel;
                    }        
                }
                else
                {
                    if (uriVsNode.TryGetValue(schemaClass.ParentClass.BaseUri, 
                        out var parentNode))
                    {
                        parentNode.AddChild(classNode);
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

            classNode.AddChild(propNode);
        }
    }

    private void HandleClassIndividualsNodes(CimSchemaEntityNodeModel classNode,
        IEnumerable<ICimMetaIndividual> individuals)
    {
        foreach (var individual in individuals)
        {
            var individualrefix = CimSchemaContext!
                .GetUriNamespacePrefix(individual.BaseUri);

            var individualNode = new CimSchemaEntityNodeModel(individual)
            {
                Title = $"[I] {individualrefix}:{individual.ShortName}"
            };
            classNode.AddChild(individualNode);
        }
    }

    private string GetClassTraitsString(ICimMetaClass cimMetaClass)
    {
        string traits = $"Compound={cimMetaClass.IsCompound}; "
            + $"Enum={cimMetaClass.IsEnum}; "
            + $"Datatype={cimMetaClass.IsDatatype}; "
            + $"Abstract={cimMetaClass.IsAbstract}; "
            + $"Extension={cimMetaClass.IsExtension}; "
            + $"CanCreate={CimSchemaContext?.CanCreateClass(cimMetaClass)};";

        return traits;
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
                return $"<{datatype.PrimitiveType.Name}>"; 
            }

            return $"<{datatype.ShortName}/{datatype.PrimitiveType.Name}>";
        }

        if (property.PropertyDatatype is ICimMetaClass metaClass)
        {
            return $"<{metaClass.ShortName}>";
        }

        return string.Empty;
    }

    protected ObservableCollection<CimSchemaEntityNodeModel> _NodesCache
        = new ObservableCollection<CimSchemaEntityNodeModel>();

    private string _SearchString = string.Empty;
    private bool _ShowProperties = true;
    private bool _ShowIndividuals = true;
}
