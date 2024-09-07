using System.Xml.Linq;

namespace CimBios.Core.DataProvider;

public class RdfXmlFileDataProvider : IDataProvider
{
    public RdfXmlFileDataProvider(Uri source)
    {
        _source = source;
    }

    public Uri Source 
    { get => _source; set => _source = value; }

    public object Get()
    {
        TextReader reader = new StreamReader(Source.LocalPath);
        XDocument xDocument = XDocument.Load(reader);

        return xDocument;
    }

    public void Push(object data)
    {
        if (data is XDocument xDocument)
        {
            xDocument.Save(Source.LocalPath);
        }
        else
        {
            throw new Exception("data is not XDocument");
        }
    }

    private Uri _source;
}

public class RdfXmlFileDataProviderFactory : IDataProviderFactory
{
    public IDataProvider CreateProvider(Uri source)
    {
        return new RdfXmlFileDataProvider(source);
    }
}
