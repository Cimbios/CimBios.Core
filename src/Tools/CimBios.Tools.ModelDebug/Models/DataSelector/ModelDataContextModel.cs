using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;

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

public interface IRdfSerializerFactory
{
    public RdfSerializerBase Create(ICimSchema cimSchema, 
        ICimDatatypeLib datatypeLib);
}

public class RdfXmlSerializerFactory : IRdfSerializerFactory
{
    public RdfSerializerBase Create(ICimSchema cimSchema, 
        ICimDatatypeLib datatypeLib)
    {
        return new RdfXmlSerializer(cimSchema, datatypeLib);
    }
}
