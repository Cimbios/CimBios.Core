using System.Xml.Linq;
using CimBios.Core.RdfXmlIOLib;

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
        var xDoc = GetLoadXDocAsset();
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

    private static XDocument GetLoadXDocAsset()
    {
        TextReader reader = GetLoadAsset();
        XDocument xDocument = XDocument.Load(reader);
        
        return xDocument;

    }

    private static string GetParseAsset()
    {
       return GetLoadAsset().ReadToEnd(); 
    }
}
