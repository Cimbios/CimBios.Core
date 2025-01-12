using System.Text;

namespace CimBios.Core.DataProvider;

public class FileStreamDataProvider : IDataProvider
{
    public FileStreamDataProvider(Uri source)
    {
        _source = source;
        
        _stream = File.Open(source.LocalPath, 
            FileMode.OpenOrCreate, FileAccess.ReadWrite);
    }

    public Uri Source 
    { get => _source; set => _source = value; }

    public Stream DataStream => _stream;

    public System.Type Datatype => typeof(string);

    public object Get()
    {
        TextReader reader = new StreamReader(_stream);
        return reader.ReadToEnd();
    }

    public void Push(object data)
    {
        if (data is string stringData)
        {
            byte[] buffer = Encoding.Default.GetBytes(stringData);
            _stream.Write(buffer, 0, buffer.Length);
        }
        else
        {
            throw new Exception($"data is not {Datatype.Name}");
        }
    }

    private Uri _source;
    private readonly FileStream _stream;
}

