using System;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Tools.ModelDebug.Models;

public class SchemaSelectorModel
{
    public string Title { get; }
    public ICimSchemaFactory SchemaFactory { get; }
    public ISourceSelector SourceSelector { get; }

    public SchemaSelectorModel(string title, 
        ICimSchemaFactory schemaFactory,
        ISourceSelector sourceSelector)
    {
        Title = title;
        SchemaFactory = schemaFactory;
        SourceSelector = sourceSelector;
    }
}
