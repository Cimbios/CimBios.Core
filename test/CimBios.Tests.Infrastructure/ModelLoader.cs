using System.Reflection;
using CimBios.Core.CimModel.DataModel;
using CimBios.Core.CimModel.DataModel.Document;
using CimBios.Core.CimModel.DatatypeLib;
using CimBios.Core.CimModel.DatatypeLib.Factory;
using CimBios.Core.CimModel.DatatypeLib.OID;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.Schema.RdfSchema;

namespace CimBios.Tests.Infrastructure;

public static class ModelLoader
{
    public static string CommonAssetsPath = "../../../../common_assets/";

    public static ICimDataModel LoadCimModel_v1(bool allowUnknown = false)
    {
        var schema = LoadTestCimRdfSchema();

        var typeLib = new CimDatatypeLib(schema);
        typeLib.LoadAssembly(Assembly.GetExecutingAssembly(), reset: true);

        var cimDocument = new CimDocument(schema, typeLib,
            new TextDescriptorFactory());

        cimDocument.Load(CommonAssetsPath + "ASubstation-CIMXML-FullModel-v1.xml",
            new RdfXmlSerializerFactory
            {
                Settings = new RdfSerializerSettings
                {
                    UnknownClassesAllowed = allowUnknown,
                    UnknownPropertiesAllowed = allowUnknown,
                    IncludeUnresolvedReferences = true
                }
            });

        return cimDocument;
    }

    public static ICimDataModel LoadCimModel_v1_changed(bool allowUnknown = false)
    {
        var schema = LoadTestCimRdfSchema();

        var typeLib = new CimDatatypeLib(schema);
        typeLib.LoadAssembly(Assembly.GetExecutingAssembly(), reset: true);

        var cimDocument = new CimDocument(schema, typeLib,
            new TextDescriptorFactory());

        cimDocument.Load(CommonAssetsPath + "ASubstation-CIMXML-FullModel-v1-changed.xml",
            new RdfXmlSerializerFactory
            {
                Settings = new RdfSerializerSettings
                {
                    UnknownClassesAllowed = allowUnknown,
                    UnknownPropertiesAllowed = allowUnknown,
                    IncludeUnresolvedReferences = true
                }
            });

        return cimDocument;
    }

    public static ICimDifferenceModel LoadCimDiffModel_v1()
    {
        var schema = Load552HeadersCimRdfSchema();

        var typeLib = new CimDatatypeLib(schema);
        typeLib.LoadAssembly(Assembly.GetExecutingAssembly(), reset: false);

        var cimDifferenceModel = new CimDifferenceModel(schema, typeLib,
            new TextDescriptorFactory());

        cimDifferenceModel.Load(CommonAssetsPath + "CIMXML-DifferenceModel-v1.xml",
            new RdfXmlSerializerFactory());

        return cimDifferenceModel;
    }

    public static ICimSchema LoadTestCimRdfSchema()
    {
        return LoadCimSchema(CommonAssetsPath + "Iec61970-Test-rdfs.xml",
            new CimRdfSchemaXmlFactory());
    }

    public static ICimSchema Load552HeadersCimRdfSchema()
    {
        return LoadCimSchema(CommonAssetsPath + "Iec61970-552-Headers-rdfs.xml",
            new CimRdfSchemaXmlFactory());
    }

    public static ICimDataModel CreateCimModelInstance()
    {
        var schema = LoadTestCimRdfSchema();
        var typeLib = new CimDatatypeLib(schema);
        typeLib.LoadAssembly(Assembly.GetExecutingAssembly(), reset: true);
        
        var cimDocument = new CimDocument(schema, typeLib,
            new TextDescriptorFactory());

        return cimDocument;
    }

    private static ICimSchema LoadCimSchema(string path,
        ICimSchemaFactory factory)
    {
        var cimSchema = factory.CreateSchema();

        cimSchema.Load(new StreamReader(path));
        cimSchema.Namespaces.TryAdd("cim", new Uri("http://iec.ch/TC57/CIM100#"));
        cimSchema.Namespaces.TryAdd("rf", new Uri("http://gost.ru/2019/schema-cim01#"));

        return cimSchema;
    }
}