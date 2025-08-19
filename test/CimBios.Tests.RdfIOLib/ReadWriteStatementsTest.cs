using System.Text;
using CimBios.Core.RdfIOLib;

namespace CimBios.Tests.RdfIOLib;

public class ReadWriteStatementsTest
{
    [Fact]
    public void ReadRdfXmlStatements()
    {
        var rdfReader = new RdfXmlReader();
        rdfReader.Load(new StreamReader("../../../assets/Statements.xml"));

        var rdfDocument = rdfReader.ReadAll().ToList();

        var Node = rdfDocument.FirstOrDefault();
        if (Node == null) Assert.Fail("Rdf doc is empty.");

        var NodeStatements = Node.Triples
            .FirstOrDefault(t =>
                t.Predicate.AbsoluteUri
                == "http://cim.bios/tests#Node.Statements");
        if (NodeStatements == null) Assert.Fail("Rdf doc does not contain Node.Statements.");

        Assert.True(NodeStatements.Object
                        is RdfTripleObjectStatementsContainer statementsContainer
                    && statementsContainer.RdfNodesObject.Count == 4);
    }

    [Fact]
    public void WriteRdfXmlStatements()
    {
        var rdfReader = new RdfXmlReader();
        var streamReader = new StreamReader("../../../assets/Statements.xml");
        var statementsContent = streamReader.ReadToEnd();
        rdfReader.Parse(statementsContent);
        var rdfDocument = rdfReader.ReadAll().ToList();

        var rdfWriter = new RdfXmlWriter();
        foreach (var (prefix, nsUri) in rdfReader.Namespaces) rdfWriter.AddNamespace(prefix, nsUri);

        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);
        rdfWriter.Open(stringWriter, false);
        rdfWriter.WriteAll(rdfDocument);
        var wroteContent = stringBuilder.ToString();

        var postRdfReader = new RdfXmlReader();
        postRdfReader.Parse(wroteContent);

        var postRdfDocument = postRdfReader.ReadAll().ToList();

        var Node = postRdfDocument.FirstOrDefault();
        if (Node == null) Assert.Fail("Rdf doc is empty.");

        var NodeStatements = Node.Triples
            .FirstOrDefault(t =>
                t.Predicate.AbsoluteUri
                == "http://cim.bios/tests#Node.Statements");
        if (NodeStatements == null) Assert.Fail("Rdf doc does not contain Node.Statements.");

        Assert.True(NodeStatements.Object
                        is RdfTripleObjectStatementsContainer statementsContainer
                    && statementsContainer.RdfNodesObject.Count == 4);
    }
}