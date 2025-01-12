using CimBios.Core.CimModel.Document;

namespace CimBios.Tools.ModelDebug.Models;

public class ModelDataContextModel
{
    public string Title { get; }
    public IModelObjectsProviderFactory ModelDataContextFactory { get; }
    public ISourceSelector SourceSelector { get; }

    public ModelDataContextModel(string title, 
        IModelObjectsProviderFactory modelDataContextFactory,
        ISourceSelector sourceSelector)
    {
        Title = title;
        ModelDataContextFactory = modelDataContextFactory;
        SourceSelector = sourceSelector;
    }
}

