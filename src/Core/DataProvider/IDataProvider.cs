namespace CimBios.Core.DataProvider;

public interface IDataProvider
{
    public Uri Source { get; set; }

    public abstract object Get();
    public abstract void Push(object data);
}
