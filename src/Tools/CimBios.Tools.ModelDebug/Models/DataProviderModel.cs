using CimBios.Core.DataProvider;

namespace CimBios.Tools.ModelDebug.Models;

public class DataProviderModel
{
    public string Title { get; }
    public IDataProviderFactory ProviderFactory { get; }
    public ISourceSelector SourceSelector { get; }

    public DataProviderModel(string title, 
        IDataProviderFactory providerFactory,
        ISourceSelector sourceSelector)
    {
        Title = title;
        ProviderFactory = providerFactory;
        SourceSelector = sourceSelector;
    }
}

