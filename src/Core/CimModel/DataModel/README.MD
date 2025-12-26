# CimBios.Core.CimModel.DataModel

#### CimBios.Core - a swiss army knife 🛠 in the world of `CIM (IEC61970)` models
#### Check out general feautures:
- `CIM FullModel` serializing/deserializing tools
`Supports custom data schemas - information model profiles`
- `CIM difference model` utilities support
	- Applying difference to model
	- Compare models
	- Track model changes
	- (De)serializing
- `Metamodel` system based on `CIM UML` standards description
	- Classes, inheritance
	- Properties (attributes, associations, compounds, enums)
	- Datatypes
	- Stereotypes
	- Packages
	- Extensions and splitted namespaces
- `Validation` features
- Flexible core architecture
`You can design your own type libs, schemas, serializers, validation rules`

------------

### Getting Started
#### 1. Create a New DotNET Project

If you don't already have a dotnet project, create one in Visual Studio.

#### 2. Install NuGet Package
Inatall `CimBios.Core.CimModel.DataModel` NuGet package to your app with your preferred method. Here is the one using NuGet Package Manager:

```bash
Install-Package CimBios.Core.CimModel.DataModel
```
#### 3. Usage example

- 🆕 **Create new FullModel**

```csharp
using CimBios.Core.CimModel.DataModel.Document;

// 💡 Load RDF schema - IM profile
var schema = new CimRdfSchemaXmlFactory().CreateSchema();
schema.Load(new StreamReader($"{path}"));

// 💡 Create type lib instance
// Schema is used to check registering assembly types
var typeLib = new CimDatatypeLib(schema);

// 💡 Create OID descriptor factory - CIM objects identifiers factory. Uuid for example.
var oidDescriptorFactory = new UuidDescriptorFactory();

// 💡 Create CIM FullModel document
var document = new CimDocument(schema, typeLib, oidDescriptorFactory);

// 💡 Set FullModel header
var fullModel = document.CreateObject<FullModel>(new UuidDescriptor());
fullModel.created = DateTime.Now;
fullModel.description = "Hello CIM!";
```

- 📦 **Model storage accessing**

```csharp
// 💡 Create new object
var geoRegion = document.CreateObject<GeographicalRegion>(new UuidDescriptor()); // via typelib

var geoMetaClass = schema.TryGetResource<ICimMetaClass>(new
	Uri("http://iec.ch/TC57/CIM100#GeographicalRegion"));
var geoRegion2 = document.CreateObject(new UuidDescriptor(), geoMetaClass!); // via metamodel

// Create object w/ concrete identifier
var baseVoltage115 = document.CreateObject<BaseVoltage>(
    new UuidDescriptor(new Guid("3f3ac268-6c9f-44a6-98b8-c3e53ee42f1a")));

// 💡 Remove object
document.RemoveObject(geoRegion2);
document.RemoveObject(new UuidDescriptor(new Guid("3f3ac268-6c9f-44a6-98b8-c3e53ee42f1a")));

// 💡 Get objects
var all = document.GetAllObjects(); // all

// All objects of concrete class
var allGeoRegions = document.GetObjects<GeographicalRegion>(); // via typelib
var allGeoRegions2 = document.GetObjects(geoMetaClass); // via metamodel

// Get concrete object
var modelObject = document.GetObject("3f3ac268-6c9f-44a6-98b8-c3e53ee42f1a");
var typedObject = document.GetObject<IdentifiedObject>("3f3ac268-6c9f-44a6-98b8-c3e53ee42f1a");

```

- 💾 **Load and save FullModel**

```csharp
// ⚠ schema, typeLib, oidDescriptorFactory definition described in `Create new FullModel` example
// 💡 Create CIM FullModel document
var document = new CimDocument(schema, typeLib, oidDescriptorFactory);

// 💡 Create RDF serializer factory dependant on CIM model format
var serializerFactory = new RdfXmlSerializerFactory();

document.Load($"{model}", serializerFactory);
```

- ➡ **Import another FullModel**

```csharp
// 💡 Import other model objects
document1.ImportModelObjects(document2);
```

- 💾 **Load and save DifferenceModel**

**`⚠ Insure CimBios.Core.CimModel.DataModel refence added to project explicitly!`**
```csharp
var coreLib = new CoreDatatypeLibFactory().Create(); // core typelib, which contains all necessary classes
var diffModel = new CimDifferenceModel(coreLib.Schema, coreLib, oidDescriptorFactory);

// manage default schema w/ needed namespaces and prefix
coreLib.Schema.Namespaces.Add("cim", new Uri("http://iec.ch/TC57/CIM100#"));

diffModel.Load($"{diffmodel}", new RdfXmlSerializerFactory());
diffModel.Save($"{diffmodel}", new RdfXmlSerializerFactory());
```

- ➡ **Apply DifferenceModel to FullModel**

```csharp
// 💡 Apply DifferenceModel
document.ApplyDifferenceModel(diffModel);
```

- 🔥 **Subscribe DifferenceModel on model changes**

```csharp
// 💡 Subscribe DifferenceModel
diffModel.SubscribeToDataModelChanges(document);
```

#### ⚡ *To get more usage details check out .NET demo console app in [GitHub repository](https://github.com/belizahrt/CimBiosCoreExampleApp)* ⚡

#### 4. Diagnostics logging
Base type lib class supports logging via Serilog `ILogger` interface:

```csharp
// Pass ILogger instance to CimDocumentBase constructor:
 var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

var document = new CimDocument(schema, typeLib, oidDescriptorFactory)
```
