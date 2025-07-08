using System;
using System.IO;
using Avalonia.Platform;
using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.CimDifferenceModel;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.Schema.RdfSchema;
using CimBios.Tools.ModelDebug.ViewModels;
using CimBios.Utils.ClassTraits.CanLog;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CimBios.Tools.ModelDebug.Services;

public class CimModelLoaderService : ObservableObject
{
    private ICimDataModel? _dataContext;
    private ICimDifferenceModel? _localDifferences;

    public ICimDataModel? DataContext
    {
        get => _dataContext;
        private set
        {
            if (_dataContext == value) return;

            _dataContext = value;
            OnPropertyChanged();
        }
    }

    public ICimDifferenceModel? LocalDifferences
    {
        get => _localDifferences;
        private set
        {
            if (_localDifferences == value) return;

            _localDifferences = value;
            OnPropertyChanged();
        }
    }

    public void LoadModelFromFile(
        string modelPath, string schemaPath,
        IOIDDescriptorFactory descriptorFactory,
        ICimSchemaFactory schemaFactory,
        IRdfSerializerFactory serializerFactory,
        RdfSerializerSettings serializerSettings,
        out ILog log)
    {
        log = new PlainLogView(this);

        try
        {
            var schema = schemaFactory.CreateSchema();
            schema.Load(new StreamReader(schemaPath));

            var typeLib = new CimDatatypeLib(schema);

            var model = new CimDocument(schema, typeLib, descriptorFactory);

            serializerFactory.Settings = serializerSettings;
            model.Load(modelPath, serializerFactory);

            log.FlushFrom(schema.Log);
            log.FlushFrom(typeLib.Log);
            log.FlushFrom(model.Log);

            DataContext = model;

            InitializeLocalDifferences(model);
        }
        catch (Exception ex)
        {
            GlobalServices.ProtocolService.Error($"Loading CIM failed: {ex.Message}", "");
        }
        finally
        {
            GlobalServices.ProtocolService.AddFromLibLog(log, $"Load CIM model {modelPath}");
        }
    }

    public void SaveModelToFile(string modelPath, string schemaPath,
        ICimSchemaFactory schemaFactory,
        IRdfSerializerFactory serializerFactory,
        RdfSerializerSettings serializerSettings,
        out ILog log)
    {
        log = new PlainLogView(this);

        if (DataContext is not CimDocument model)
        {
            GlobalServices.ProtocolService
                .Error("Saving canceled: no document load", "");

            return;
        }

        try
        {
            var schema = schemaFactory.CreateSchema();
            schema.Load(new StreamReader(schemaPath));

            serializerFactory.Settings = serializerSettings;
            model.Save(modelPath, serializerFactory, schema);
            log.FlushFrom(schema.Log);
            log.FlushFrom(model.Log);
        }
        catch (Exception ex)
        {
            GlobalServices.ProtocolService
                .Error($"Saving CIM failed: {ex.Message}", "");
        }
        finally
        {
            GlobalServices.ProtocolService.AddFromLibLog(log,
                $"Save CIM model {modelPath}");
        }
    }

    public void SaveLocalDifferencesToFile(string modelPath)
    {
        if (_localDifferences is not CimDifferenceModel differenceModel) return;

        differenceModel.Save(modelPath, new RdfXmlSerializerFactory());
    }

    private void InitializeLocalDifferences(ICimDataModel model)
    {
        var diffSchema = new CimRdfSchemaXmlFactory().CreateSchema();

        var diffSchemaResource = AssetLoader
            .Open(new Uri("avares://CimBios.Tools.ModelDebug/Assets/Iec61970-552-Headers-rdfs.xml"));
        using TextReader schemaReader = new StreamReader(diffSchemaResource);

        diffSchema.Load(schemaReader);

        var diffTypeLib = new CimDatatypeLib(diffSchema);

        _localDifferences = new CimDifferenceModel(diffSchema, diffTypeLib, model);
    }
}