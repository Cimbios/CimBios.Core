using System.IO;
using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;
using CimBios.Utils.ClassTraits.CanLog;

namespace CimBios.Tools.ModelDebug;

public static class CimDataModelProvider
{
    public static ICimDataModel LoadFromFile(
        string modelPath, string schemaPath,
        IOIDDescriptorFactory descriptorFactory,
        ICimSchemaFactory schemaFactory, 
        IRdfSerializerFactory serializerFactory,
        RdfSerializerSettings serializerSettings,
        out ILog log)
    {
        var schema = schemaFactory.CreateSchema();
        schema.Load(new StreamReader(schemaPath));

        var typeLib = new CimDatatypeLib(schema);

        var model = new CimDocument(schema, typeLib, descriptorFactory);

        serializerFactory.Settings = serializerSettings;
        model.Load(modelPath, serializerFactory);

        log = new PlainLogView(model);
        log.FlushFrom(schema.Log);
        log.FlushFrom(typeLib.Log);
        log.FlushFrom(model.Log);

        return model;
    }
}
