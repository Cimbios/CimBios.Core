using System;
using System.Linq;
using CimBios.Core.CimModel.CimDatatypeLib;

namespace CimBios.Tools.ModelDebug.Models;

public class CimObjectDataTreeModel : LinkedNodeModel
{
    public IModelObject ModelObject { get; set; }

    public string Uuid { get => ModelObject.Uuid; }

    public string Name 
    {
        get
        {
            if (ModelObject.ObjectData.Attributes.Contains("name"))
            {
                return ModelObject.ObjectData.GetAttribute<string>("name");
            }

            return "noname";
        }
    }

    public CimObjectDataTreeModel(IModelObject modelObject) : base()
    {
        ModelObject = modelObject;
    }
}
