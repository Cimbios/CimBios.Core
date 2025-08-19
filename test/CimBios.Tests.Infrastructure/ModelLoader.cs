using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.CimDifferenceModel;
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
        var cimDocument = new CimDocument(schema, new CimDatatypeLib(schema),
            new TextDescriptorFactory());

        return cimDocument;
    }

    private static ICimSchema LoadCimSchema(string path,
        ICimSchemaFactory factory)
    {
        var cimSchema = factory.CreateSchema();

        cimSchema.Load(new StreamReader(path));

        return cimSchema;
    }
}