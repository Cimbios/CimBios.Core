using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CimBios.Core.CimModel.Context;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.RdfXmlIOLib;
using CimBios.Tools.ModelDebug.Models;

namespace CimBios.Tools.ModelDebug.ViewModels;

public class CimSchemaTreeViewModel : ViewModelBase
{
    public ObservableCollection<TreeViewNodeModel> Nodes { get; } 

    private ICimSchema? CimSchemaContext { get; set; }

    public CimSchemaTreeViewModel()
    {
        Nodes = new ObservableCollection<TreeViewNodeModel>();

        SubscribeModelContextLoad();
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

        var uriVsNode = new Dictionary<Uri, CimSchemaEntityNodeModel>(
            new RdfUriComparer());

        foreach (var schemaClass in CimSchemaContext.Classes)
        {
            if (uriVsNode.ContainsKey(schemaClass.BaseUri))
            {
                continue;
            }

            var node = new CimSchemaEntityNodeModel(schemaClass, 
                CimSchemaContext.GetUriNamespacePrefix(schemaClass.BaseUri));
            uriVsNode.Add(schemaClass.BaseUri, node);
        }

        foreach (var schemaClass in CimSchemaContext.Classes)
        {
            if (uriVsNode.TryGetValue(schemaClass.BaseUri, 
                out var classNode))
            {
                if (schemaClass.ParentClass == null)
                {
                    Nodes.Add(classNode);
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
}
