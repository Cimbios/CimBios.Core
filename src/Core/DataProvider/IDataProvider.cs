namespace CimBios.Core.DataProvider;

public interface IDataProvider
{
    public Uri Source { get; set; }

    public abstract object Get();
    public abstract void Push(object data);
}

/// <summary>
/// Factory method interface for abstract data provider activation.
/// </summary>
public interface IDataProviderFactory
{
    /// <summary>
    /// Create IDataProvider instance.
    /// </summary>
    /// <param name="source">URI of data source.</param>
    /// <returns>IDataProvider instance.</returns>
    IDataProvider CreateProvider(Uri source);
}
