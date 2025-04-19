using System.IO;
using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;
using CimBios.Utils.ClassTraits.CanLog;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CimBios.Tools.ModelDebug.Services;

public class CimModelLoaderService : ObservableObject
{
    public ICimDataModel? DataContext 
    {
        get => _DataContext;
        private set
        {
            if (_DataContext == value)
            {
                return;
            }

            _DataContext = value;
            OnPropertyChanged();
        }
    }

    public void LoadFromFile(
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

        DataContext = model;
    }

    private ICimDataModel? _DataContext = null;
}
