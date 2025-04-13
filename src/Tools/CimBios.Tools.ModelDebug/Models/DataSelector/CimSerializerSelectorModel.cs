using CimBios.Core.CimModel.RdfSerializer;

namespace CimBios.Tools.ModelDebug.Models;

public class CimSerializerSelectorModel(string title,
    IRdfSerializerFactory rdfSerializerFactory)
{
    public string Title { get; } = title;
    public IRdfSerializerFactory RdfSerializerFactory { get; } 
        = rdfSerializerFactory;
}
