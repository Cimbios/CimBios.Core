# CimBios.Core.CimModel.RdfSerializer

CimBios.Core.CimModel.RdfSerializer is a library for serializing `CIM (IEC61970)` class objects according to the data schema and type library in [CimBios.Core libraries](https://github.com/Cimbios/CimBios.Core).

## Getting Started

### 1. Create a New DotNET Project
If you don't already have a dotnet project, create one in Visual Studio.
### 2: Install NuGet Package
Inatall `CimBios.Core.CimModel.RdfSerializer` NuGet package to your app with your preferred method. Here is the one using NuGet Package Manager:

```bash
Install-Package CimBios.Core.CimModel.RdfSerializer
```
### 3. Usage

```csharp
// CIM objects deserialization from CIMXML file via IRdfSerializerFactory

var schema = new CimRdfSchemaXmlFactory().CreateSchema();
schema.Load(new StreamReader("schema.rdfs"));
var typeLib = new CimDatatypeLib(schema);

var serializerFactory = new RdfXmlSerializerFactory();
var serializer = serializerFactory.Create(
	schema, // CIM data schema
	typeLib, // CIM types library
	new UuidDescriptorFactory()); // CIM objects GUID identifiers support
	
result = serializer.Deserialize(new StreamReader("model.xml"));

// CIM objects serialization to CIMXML file
serializer.Serialize(new StreamWriter("model.xml"), result.ModelObjects);
```

------------

Deserialization result contains:
- Parsed namespaces (list of `(prefix, URI)` pairs)
- Deserialized objects enumeration
- Unresolved model object references enumeration

### 4. Serializer settings
There are several settings for (de)serilization proccess:

```csharp
// Pass RdfSerializerSettings in-field of IRdfSerializerFactory initialization:
var serializerFactory = new RdfXmlSerializerFactory()
{
	 Settings = new RdfSerializerSettings
	{	// default values are presented
    	UnknownClassesAllowed = false, // Allow to handle not in schema classes
    	UnknownPropertiesAllowed = false, // Allow to handle not in schema properties
    	IncludeUnresolvedReferences = true, // Save unresolved references in model
		IncludeBaseNamespace = false, // Include xml:base namespace in output model
		NormalizeIris = true, // Try to change absolute URI with namespace prefix
		RdfIRIModeKind = RdfIRIModeKind.About // rdf:about of rdf:ID identifiers mode
    }
};
```

### 5. Diagnostics logging
Base serializer class supports logging via Serilog `ILogger` interface:

```csharp
// Pass ILogger instance to IRdfSerializerFactory Create method:
 var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

var serializer = serializerFactory.Create(schema, typelib, oidDescriptorFactory, logger);
```

------------

### Supported serializers
- RDF-XML
