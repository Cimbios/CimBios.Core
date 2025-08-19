namespace CimBios.Core.DataProvider;

public interface IDataProvider
{
    public Uri Source { get; }
    public Stream DataStream { get; }
    public Type Datatype { get; }

    public object Get();
    public void Push(object data);
}