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
    private ICimDifferenceModel? _differencesContext;
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

    public ICimDifferenceModel? DifferencesContext
    {
        get => _differencesContext;
        private set
        {
            if (_differencesContext == value) return;

            _differencesContext = value;
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

    public ICimDifferenceModel? LoadDifferenceModelFromFile(
        string modelPath, 
        IOIDDescriptorFactory descriptorFactory,
        IRdfSerializerFactory serializerFactory, 
        out ILog log)
    {
        log = new PlainLogView(this);

        try
        {
            var diffSchema = MakeDifferencesSchema();
            var diffTypeLib = new CimDatatypeLib(diffSchema);

            var diffModel = new CimDifferenceModel(diffSchema, 
                diffTypeLib, descriptorFactory);
            
            diffModel.Load(modelPath, serializerFactory);
            log.FlushFrom(diffModel.Log);
            
            DifferencesContext = diffModel;
            
            return diffModel;
        }
        catch (Exception ex)
        {
            GlobalServices.ProtocolService
                .Error($"Loading differences failed: {ex.Message}", "");
        }
        finally
        {
            GlobalServices.ProtocolService.AddFromLibLog(log,
                $"Difference model loaded {modelPath}");
        }
        
        return null;
    }

    public void SaveDifferenceModelToFile(
        ICimDifferenceModel differenceModel,
        string modelPath,
        IRdfSerializerFactory serializerFactory,
        out ILog log)
    {
        log = new PlainLogView(this);

        try
        {
            if (differenceModel is not CimDifferenceModel differenceModelDoc) return;
            
            differenceModelDoc.Save(modelPath, serializerFactory);
            log.FlushFrom(differenceModelDoc.Log);
        }
        catch (Exception ex)
        {
            GlobalServices.ProtocolService
                .Error($"Saving differences failed: {ex.Message}", "");
        }
        finally
        {
            GlobalServices.ProtocolService.AddFromLibLog(log,
                $"Difference model saved {modelPath}");
        }
    }

    public void SaveLocalDifferencesToFile(string modelPath)
    {
        if (_localDifferences is not CimDifferenceModel differenceModel) return;

        differenceModel.Save(modelPath, new RdfXmlSerializerFactory());
    }

    public ICimDifferenceModel? CompareDataContextWith(string modelPath, 
        string schemaPath,
        IOIDDescriptorFactory descriptorFactory,
        ICimSchemaFactory schemaFactory,
        IRdfSerializerFactory serializerFactory,
        RdfSerializerSettings serializerSettings,
        out ILog log)
    {        
        log = new PlainLogView(this);
        
        if (DataContext == null) return null;

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

            var diffSchema = MakeDifferencesSchema();
            var diffTypeLib = new CimDatatypeLib(diffSchema);

            var diffModel = new CimDifferenceModel(diffSchema, 
                diffTypeLib, descriptorFactory);
            
            diffModel.CompareDataModels(DataContext, model);
            
            log.FlushFrom(diffModel.Log);
            
            return diffModel;
        }
        catch (Exception ex)
        {
            GlobalServices.ProtocolService.Error($"Compare failed: {ex.Message}", "");
        }
        finally
        {
            GlobalServices.ProtocolService.AddFromLibLog(log, $"Compared with {modelPath}");
        }
        
        return null;
    }

    private void InitializeLocalDifferences(ICimDataModel model)
    {
        var diffSchema = MakeDifferencesSchema();
        var diffTypeLib = new CimDatatypeLib(diffSchema);

        _localDifferences = new CimDifferenceModel(diffSchema, diffTypeLib, model);
    }

    private static ICimSchema MakeDifferencesSchema()
    {
        var diffSchema = new CimRdfSchemaXmlFactory().CreateSchema();

        var diffSchemaResource = AssetLoader
            .Open(new Uri("avares://CimBios.Tools.ModelDebug/Assets/Iec61970-552-Headers-rdfs.xml"));
        using TextReader schemaReader = new StreamReader(diffSchemaResource);

        diffSchema.Load(schemaReader);
        
        return diffSchema;
    }
}