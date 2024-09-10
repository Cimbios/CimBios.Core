using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CimBios.Core.CimModel.Context;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.RdfXmlIOLib;
using CimBios.Tools.ModelDebug.Models;
using CommunityToolkit.Mvvm.Input;

namespace CimBios.Tools.ModelDebug.ViewModels;

public class CimSchemaTreeViewModel : ViewModelBase, INotifyPropertyChanged
{
    public IEnumerable<TreeViewNodeModel> Nodes 
    { 
        get
        {
            return _NodesCache;
        }
    } 

    public TreeViewNodeModel? SelectedItem { 
        get => _SelectedItem; 
        set
        {
            _SelectedItem = value;
            OnPropertyChanged(nameof(SelectedItem));   
        }
    }

    public string SearchString 
    { 
        get => _SearchString; 
        set
        {
            _SearchString = value;

            ApplyFilter();

            OnPropertyChanged(nameof(SearchString));
            OnPropertyChanged(nameof(Nodes));
        }
    } 

    public bool ShowProperties 
    { 
        get => _ShowProperties; 
        set
        {
            _ShowProperties = value;
            OnPropertyChanged(nameof(ShowProperties));
        }
    }

    public bool ShowIndividuals 
    { 
        get => _ShowIndividuals; 
        set
        {
            _ShowIndividuals = value;
            OnPropertyChanged(nameof(ShowIndividuals));
        }
    }

    public AsyncRelayCommand ExpandAllNodesCommand { get; }

    public AsyncRelayCommand UnexpandAllNodesCommand { get; }

    private ICimSchema? CimSchemaContext { get; set; }

    public CimSchemaTreeViewModel()
    {
        _NodesCache = new ObservableCollection<TreeViewNodeModel>();
        _NodesCache.CollectionChanged += NodesCache_CollectionChanged;

        ExpandAllNodesCommand = new AsyncRelayCommand
            (() => DoExpandAllNodes(true));

        UnexpandAllNodesCommand = new AsyncRelayCommand
            (() => DoExpandAllNodes(false));

        SubscribeModelContextLoad();
    }

    private void ApplyFilter()
    {
        var nodesStack = new Stack<TreeViewNodeModel>(Nodes);

        var visited = new HashSet<TreeViewNodeModel>();
        while (nodesStack.TryPop(out var node))
        {
            if (SearchString.Trim() == string.Empty)
            {
                node.IsVisible = true;
            }
            else
            {
                if (node.Title.Contains(SearchString))
                {
                    visited.Add(node);
                    var parent = node.ParentNode;
                    while (parent != null)
                    {
                        visited.Add(parent);
                        parent.IsVisible = true;
                        parent.IsExpanded = true;
                        parent = parent.ParentNode;
                    }
                }
                else
                {
                    if (visited.Contains(node) == false)
                    {
                        node.IsVisible = false;
                    }
                }
            }

            node.SubNodes.ToList().ForEach(n => nodesStack.Push(n));
        }     
    }

    private Task DoExpandAllNodes(bool IsExpand)
    {
        var nodesStack = new Stack<TreeViewNodeModel>(Nodes);

        while (nodesStack.TryPop(out var node))
        {
            node.IsExpanded = IsExpand;
            node.SubNodes.ToList().ForEach(n => nodesStack.Push(n));
        }

        return Task.CompletedTask;
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
                    _NodesCache.Add(classNode);

                    if (SelectedItem == null && classNode.SubNodes.Count() != 0)
                    {
                        SelectedItem = classNode.SubNodes.FirstOrDefault();
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
            classNode.AddChild(individualNode);
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

    private ObservableCollection<TreeViewNodeModel> _NodesCache;
    private TreeViewNodeModel? _SelectedItem;
    private string _SearchString = string.Empty;
    private bool _ShowProperties = true;
    private bool _ShowIndividuals = true;
}
