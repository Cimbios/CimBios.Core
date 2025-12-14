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
using CommunityToolkit.Mvvm.ComponentModel;
using Serilog;

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
        ILogger? logger=null)
    {
        logger ??= GlobalServices.Logger;

        try
        {
            var schema = schemaFactory.CreateSchema(logger);
            schema.Load(new StreamReader(schemaPath));

            var typeLib = new CimDatatypeLib(schema, logger);

            var model = new CimDocument(schema, typeLib, descriptorFactory, logger);

            serializerFactory.Settings = serializerSettings;
            model.Load(modelPath, serializerFactory);

            DataContext = model;

            InitializeLocalDifferences(model, logger);
        }
        catch (Exception ex)
        {
            GlobalServices.ProtocolService.Error($"Loading CIM failed: {ex.Message}", "");
        }
        finally
        {
            GlobalServices.ProtocolService.Info($"Load CIM model {modelPath}", "Loader");
        }
    }

    public void SaveModelToFile(string modelPath, string schemaPath,
        ICimSchemaFactory schemaFactory,
        IRdfSerializerFactory serializerFactory,
        RdfSerializerSettings serializerSettings,
        ILogger? logger=null)
    {
        logger ??= GlobalServices.Logger;

        if (DataContext is not CimDocument model)
        {
            GlobalServices.ProtocolService
                .Error("Saving canceled: no document load", "");

            return;
        }

        try
        {
            var schema = schemaFactory.CreateSchema(logger);
            schema.Load(new StreamReader(schemaPath));

            serializerFactory.Settings = serializerSettings;
            model.Save(modelPath, serializerFactory, schema);
        }
        catch (Exception ex)
        {
            GlobalServices.ProtocolService
                .Error($"Saving CIM failed: {ex.Message}", "");
        }
        finally
        {
            GlobalServices.ProtocolService.Info($"Save CIM model {modelPath}", "Loader");
        }
    }

    public ICimDifferenceModel? LoadDifferenceModelFromFile(
        string modelPath, 
        IOIDDescriptorFactory descriptorFactory,
        IRdfSerializerFactory serializerFactory, 
        ILogger? logger=null)
    {
        logger ??= GlobalServices.Logger;

        try
        {
            var diffSchema = MakeDifferencesSchema(logger);
            var diffTypeLib = new CimDatatypeLib(diffSchema, logger);

            var diffModel = new CimDifferenceModel(diffSchema, 
                diffTypeLib, descriptorFactory, logger);
            
            diffModel.Load(modelPath, serializerFactory);
            
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
            GlobalServices.ProtocolService.Info($"Difference model loaded {modelPath}", "Loader");
        }
        
        return null;
    }

    public void SaveDifferenceModelToFile(
        ICimDifferenceModel differenceModel,
        string modelPath,
        IRdfSerializerFactory serializerFactory,
        ILogger? logger=null)
    {
        try
        {
            if (differenceModel is not CimDifferenceModel differenceModelDoc) return;
            
            differenceModelDoc.Save(modelPath, serializerFactory);
        }
        catch (Exception ex)
        {
            GlobalServices.ProtocolService
                .Error($"Saving differences failed: {ex.Message}", "");
        }
        finally
        {
            GlobalServices.ProtocolService.Info($"Difference model saved {modelPath}", "Loader");
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
        ILogger? logger=null)
    {        
        logger ??= GlobalServices.Logger;
        
        if (DataContext == null) return null;

        try
        {
            var schema = schemaFactory.CreateSchema(logger);
            schema.Load(new StreamReader(schemaPath));

            var typeLib = new CimDatatypeLib(schema, logger);
            var model = new CimDocument(schema, typeLib, descriptorFactory, logger);

            serializerFactory.Settings = serializerSettings;
            model.Load(modelPath, serializerFactory);

            var diffSchema = MakeDifferencesSchema(logger);
            var diffTypeLib = new CimDatatypeLib(diffSchema);

            var diffModel = new CimDifferenceModel(diffSchema,
                diffTypeLib, descriptorFactory, logger);

            diffModel.CompareDataModels(DataContext, model);


            return diffModel;
        }
        catch (Exception ex)
        {
            GlobalServices.ProtocolService.Error($"Compare failed: {ex.Message}", "");
        }
        finally
        {
            GlobalServices.ProtocolService.Info($"Compared with {modelPath}", "Loader");
        }
        
        return null;
    }

    private void InitializeLocalDifferences(ICimDataModel model, ILogger logger)
    {
        var diffSchema = MakeDifferencesSchema(logger);
        var diffTypeLib = new CimDatatypeLib(diffSchema);

        _localDifferences = new CimDifferenceModel(diffSchema, diffTypeLib, model);
    }

    private static ICimSchema MakeDifferencesSchema(ILogger logger)
    {
        var diffSchema = new CimRdfSchemaXmlFactory().CreateSchema(logger);

        var diffSchemaResource = AssetLoader
            .Open(new Uri("avares://CimBios.Tools.ModelDebug/Assets/Iec61970-552-Headers-rdfs.xml"));
        using TextReader schemaReader = new StreamReader(diffSchemaResource);

        diffSchema.Load(schemaReader);
        
        return diffSchema;
    }
}