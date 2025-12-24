# CimBios.Core.RdfIOLib

CimBios.Core.RdfIOLib is a Light-weight Rdf serialization/deserialization library. Provides general rdf entities such as rdf node, triples, resource identifiers, statements, blank nodes, e.t.c.

### Supported serializers
- XML

### Usage
```csharp
using System.Text;
using CimBios.Core.RdfIOLib;
using CimBios.Core.RdfIOLib.RdfXml;

// reading from stream (file):
var rdfReader = new RdfXmlReader();
rdfReader.Load(new StreamReader("graph.xml"));
var nodes = rdfReader.ReadAll().ToList();

// writing:
var rdfWriter = new RdfXmlWriter();

// namespaces managment
foreach (var (prefix, nsUri) in rdfReader.Namespaces)
	rdfWriter.AddNamespace(prefix, nsUri);

rdfWriter.Open(new StreamWriter("graph2.xml"));
rdfWriter.WriteAll(nodes);
```

