using System;
using System.Linq;
using CimBios.Core.CimModel.CimDatatypeLib;

namespace CimBios.Tools.ModelDebug.Models;

public class CimObjectDataTreeModel : TreeViewNodeModel
{
    public IModelObject ModelObject { get; set; }

    public string Uuid { get => ModelObject.Uuid; }

    public string Name 
    {
        get
        {
            if (ModelObject.ObjectData.Attributes.Contains("name"))
            {
                return ModelObject.ObjectData
                    .GetAttribute<string>("name") ?? "noname";
            }

            return "noname";
        }
    }

    public CimObjectDataTreeModel(IModelObject modelObject) : base()
    {
        ModelObject = modelObject;

        Title = $"{Uuid}: {Name}";
    }
}
