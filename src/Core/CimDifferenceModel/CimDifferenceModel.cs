using System.Text;
using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.DifferenceModel;

public class CimDifferenceModel : ICimDifferenceModel
{
    public CimDifferenceModel(RdfSerializerBase rdfSerializer)
    {
        _serializer = rdfSerializer;
        _serializer.Settings.UnknownClassesAllowed = true;
        _serializer.Settings.UnknownPropertiesAllowed = true;
    }

    public CimDifferenceModel(RdfSerializerBase rdfSerializer, 
        ICimDataModel cimDataModel)
        : this(rdfSerializer)
    {
        ExtractFromDataModel(cimDataModel);
    }

    public void Load(StreamReader streamReader)
    {
        if (streamReader == null || _serializer == null)
        {
            // _Log.NewMessage(
            //     "CimDocument: Stream reader or serializer has not been initialized!",
            //     LogMessageSeverity.Error
            // );            

            return;
        }

        try
        {
            var serialized = _serializer.Deserialize(streamReader);
            var _objects = serialized.ToDictionary(k => k.OID, v => v);
            
            _internalDifferenceModel = _objects.Values
                .OfType<CimModel.CimDatatypeLib.Headers552.DifferenceModel>()
                .FirstOrDefault();
        }
        catch (Exception ex)
        {
            // _Log.NewMessage(
            //     "CimDocument: Deserialization failed.",
            //     LogMessageSeverity.Critical,
            //     ex.Message
            // );
        }
        finally
        {        
            

            streamReader.Close();
        }
    }

    public void Load(string path)
    {
        throw new NotImplementedException();
    }

    public void Parse(string content, Encoding? encoding = null)
    {
        throw new NotImplementedException();
    }

    public void Save(StreamWriter streamWriter)
    {
        throw new NotImplementedException();
    }

    public void Save(string path)
    {
        throw new NotImplementedException();
    }

    public void ExtractFromDataModel(ICimDataModel cimDataModel)
    {
        throw new NotImplementedException();
    }

    public void InvalidateDataWithModel(ICimDataModel cimDataModel)
    {
        throw new NotImplementedException();
    }

    public void ResetAll()
    {
        InitInternalDifferenceModel();
    }

    private void InitInternalDifferenceModel()
    {
        var diffModelClass = _schema.TryGetResource<ICimMetaClass>(
            new("http://iec.ch/TC57/61970-552/DifferenceModel/1#DifferenceModel"));

        if (diffModelClass == null)
        {
            throw new NotSupportedException("No dm:DifferenceModel class in schema!");
        }

        var diffModelInstance = _serializer.TypeLib.CreateInstance(
            new ModelObjectFactory(),
            Guid.NewGuid().ToString(),
            diffModelClass,
            isAuto: false
        ) as CimModel.CimDatatypeLib.Headers552.DifferenceModel;

        if (diffModelInstance == null)
        {
            throw new NotSupportedException("dm:DifferenceModel instance initialization failed!");
        }

        _internalDifferenceModel = diffModelInstance;
    }

    private CimModel.CimDatatypeLib.Headers552.DifferenceModel? 
        _internalDifferenceModel = null;

    private RdfSerializerBase _serializer;

    private ICimSchema _schema => _serializer.Schema;
}
