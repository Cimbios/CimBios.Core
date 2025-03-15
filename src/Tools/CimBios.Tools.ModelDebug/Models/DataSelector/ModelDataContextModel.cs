using CimBios.Core.CimModel.RdfSerializer;

namespace CimBios.Tools.ModelDebug.Models;

public class ModelDataContextModel
{
    public string Title { get; }
    public IRdfSerializerFactory RdfSerializerFactory { get; }
    public ISourceSelector SourceSelector { get; }

    public ModelDataContextModel(string title, 
        IRdfSerializerFactory rdfSerializerFactory,
        ISourceSelector sourceSelector)
    {
        Title = title;
        RdfSerializerFactory = rdfSerializerFactory;
        SourceSelector = sourceSelector;
    }
}
