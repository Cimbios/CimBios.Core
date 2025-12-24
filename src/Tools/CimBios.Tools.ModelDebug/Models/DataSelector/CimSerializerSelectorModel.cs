using CimBios.Core.CimModel.RdfSerializer;

namespace CimBios.Tools.ModelDebug.Models.DataSelector;

public class CimSerializerSelectorModel(
    string title,
    IRdfSerializerFactory rdfSerializerFactory)
{
    public string Title { get; } = title;

    public IRdfSerializerFactory RdfSerializerFactory { get; }
        = rdfSerializerFactory;
}