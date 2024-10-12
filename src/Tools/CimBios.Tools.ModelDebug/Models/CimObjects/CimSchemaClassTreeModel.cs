using System;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Tools.ModelDebug.Models.CimObjects;

public class CimSchemaClassTreeModel : LinkedNodeModel
{
    public ICimMetaClass? MetaClass { get; set; }
    //public Title
}
