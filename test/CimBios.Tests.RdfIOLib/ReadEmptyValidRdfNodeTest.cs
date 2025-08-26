using System.Xml;
using CimBios.Core.RdfIOLib;

namespace CimBios.Tests.RdfXmlIOLib;

public class ReadEmptyValidRdfNodeTest
{
    [Fact]
    public void Load()
    {
        var textReader = GetLoadAsset();
        var rdfReader = new RdfXmlReader();

        rdfReader.Load(textReader);

        var testResult = rdfReader.ReadAll();

        Assert.Empty(testResult);
    }

    [Fact]
    public void LoadXDoc()
    {
        var xDoc = GetXmlReaderAsset();
        var rdfReader = new RdfXmlReader();

        rdfReader.Load(xDoc);

        var testResult = rdfReader.ReadAll();

        Assert.Empty(testResult);
    }

    [Fact]
    public void Parse()
    {
        var rdfText = GetParseAsset();
        var rdfReader = new RdfXmlReader();

        rdfReader.Parse(rdfText);

        var testResult = rdfReader.ReadAll();

        Assert.Empty(testResult);
    }

    [Fact]
    public void HasOneRdfNs()
    {
        var rdfText = GetParseAsset();
        var rdfReader = new RdfXmlReader();

        rdfReader.Parse(rdfText);
        rdfReader.ReadAll();

        Assert.Contains("rdf", rdfReader.Namespaces.Keys);
    }

    private static TextReader GetLoadAsset()
    {
        return new StreamReader("../../../assets/EmptyRdfNode.xml");
    }

    private static XmlReader GetXmlReaderAsset()
    {
        var reader = GetLoadAsset();
        var xmlReader = XmlReader.Create(reader);

        return xmlReader;
    }

    private static string GetParseAsset()
    {
        return GetLoadAsset().ReadToEnd();
    }
}