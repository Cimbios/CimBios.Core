# CimBios.Core.CimModel.DatatypeLib

**CimBios.Core.CimModel.DatatypeLib** provides classes for representing `CIM (IEC61970)` ontology objects and providing access to their properties. Objects are constructed based on the meta model provided by the schema.

The library also provides an opportunity for custom development of a statically compiled type library that can be used in [CimBios.Core libraries](https://github.com/Cimbios/CimBios.Core).

### Getting Started
#### 1. Create a New DotNET Project

If you don't already have a dotnet project, create one in Visual Studio.

#### 2. Install NuGet Package
Inatall `CimBios.Core.CimModel.DatatypeLib` NuGet package to your app with your preferred method. Here is the one using NuGet Package Manager:

```bash
Install-Package CimBios.Core.CimModel.DatatypeLib
```
#### 3. Use to create CimDatatypeLib

```csharp
using CimBios.Core.CimModel.DatatypeLib;

var typeLib = new CimDatatypeLib(schema);

var breakerClass = schema.TryGetResource<ICimMetaClass>(
	new Uri("http://iec.ch/TC57/CIM100#Breaker"));

var breaker = typeLib.CreateInstance(breakerClass, new UuidDescriptor());

var phaseCodeEnum = schema.TryGetResource<ICimMetaClass>(
	new Uri("http://iec.ch/TC57/CIM100#PhaseCode.ABC"));

var enumValue = typeLib.CreateEnumValueInstance(phaseCodeEnum);
```
#### 4. Diagnostics logging
Base type lib class supports logging via Serilog `ILogger` interface:

```csharp
// Pass ILogger instance to CimTypeLib constructor:
 var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

var typeLib = new CimDatatypeLib(schema, logger);
```

------------


### Main concepts
#### 1. IModelObjectCore - is any identifiable model object
Any instance of `IModelObjectCore` implementing OID - unique object identifier.
It's also contains meta model data (CIM class, properties)
```csharp
var cimClassUri = modelObject.MetaClass.BaseUri;
var cimClassPropertiesUris = modelObject.MetaClass.AllProperties.Select(p => p.BaseUri);
```

#### 2. IReadOnlyModelObject provides read access to properties
```csharp
var attibute = modelObject.GetAttritute<string>("name"); // <T?> or object? primitive/compound type value
var association1 = modelObject.GetAssoc1To1("BaseVoltage"); // IModelObject? instance
var associationM = modelObject.GetAssoc1ToM("Names"); // IModelObject[] instance
```

#### 3. IModelObject provides mutation access to properties
```csharp
modelObject.SetAttritute("name", "Hello CIM!");
modelObject.SetAttritute("name", null);

modelObject.SetAssoc1To1("BaseVoltage", baseVoltageObject);
modelObject.SetAssoc1To1("BaseVoltage", null);

modelObject.AddToAssoc1ToM("Names", nameObject);
modelObject.RemoveFromAssoc1ToM("Names", nameObject);
modelObject.RemoveAllFromAssoc1ToM("Names");
```

#### 4. Strong meta-typed ModelObject
A strongly typed `ModelObject` class object strictly conforms to the data schema, containing only the properties described in the schema, and checking attribute data types, relationship multiplicity, relationship domains, etc.

#### 5. Weak meta-typed WeakModelObject
A `WeakModelObject` allows you to assign data to arbitrary properties. Data types can be arbitrary.

It is recommended to use such objects only outside the context of the main model. For example, to retrieve properties from a model that was assembled using an unknown schema

#### 6. DifferenceObject
DifferenceObject is a view of forwardDifference and reverseDifference transformations in the CIM dm:DifferenceModel context.

#### 7. C# types wrappers
The functionality of the type library allows you to create any objects of any registered classes that inherit from them, in addition to ModelObject and WeakModelObject classes.
To register types, they must inherit from ModelObject and have the CimSerializable attribute:
```csharp
[CimClass(ClassUri)]
public partial class IdentifiedObject(IOIDDescriptor oid, ICimMetaClass metaClass)
    : Resource(oid, metaClass) // Resource inherits from ModelObject
{
	// ClassUri - is mapping identifier between schema meta class and c# type
    public new const string ClassUri = "http://iec.ch/TC57/CIM100#IdentifiedObject";

    /// <summary>
    /// cim:IdentifiedObject.name attribute
    /// </summary>
    public string? name
    {
        get => GetAttribute<string?>(nameof(name));
        set => SetAttribute(nameof(name), value);
    }

	...
}
```
Add wrapper types or full assembly to type lib:
```csharp
var typeLib = new CimDatatypeLib(schema);
typeLib.RegisterType(typeof(IdentifiedObject));
typeLib.LoadAssembly(assembly);
```

### Ready to use type libs
- Internal (only core classes e.g Resource, Description, Model, FullModel, DifferenceModel)
- [GOST R 58651](https://github.com/Cimbios/CimBios.Gost58651)
- ~~CIM17~~ (Coming soon...)
