namespace CimBios.Core.DataProvider;

public interface IDataProvider
{
    public Uri Source { get; }
    public Stream DataStream { get; }
    public System.Type Datatype { get; }

    public abstract object Get();
    public abstract void Push(object data);
}

