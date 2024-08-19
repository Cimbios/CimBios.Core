using System.Xml.Linq;

namespace CimBios.Core.DataProvider;

public abstract class DataProviderBase<T>
    where T : class
{
    public Uri Source { get; set; }

    protected DataProviderBase(Uri source)
    {
        Source = source;
    }

    public abstract T Get();
    public abstract void Push(T data);
}

public class RdfXmlFileDataProvider : DataProviderBase<TextReader>
{
    public RdfXmlFileDataProvider(Uri source) : base(source)
    {
    }

    public override TextReader Get()
    {
        return new StreamReader(Source.LocalPath);
    }

    public override void Push(TextReader data)
    {
        using (data)
        using (StreamWriter sw = File.CreateText(Source.LocalPath))
        {
            string? line = data.ReadLine(); 
            while (line != null)         
            {
                Console.WriteLine(line);
                line = data.ReadLine();
            }
        }
    }
}