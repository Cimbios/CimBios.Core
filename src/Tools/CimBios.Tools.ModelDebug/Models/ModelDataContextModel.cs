using CimBios.Core.CimModel.Context;

namespace CimBios.Tools.ModelDebug.Models;

public class ModelDataContextModel
{
    public string Title { get; }
    public IModelDataContextFactory ModelDataContextFactory { get; }
    public ISourceSelector SourceSelector { get; }

    public ModelDataContextModel(string title, 
        IModelDataContextFactory modelDataContextFactory,
        ISourceSelector sourceSelector)
    {
        Title = title;
        ModelDataContextFactory = modelDataContextFactory;
        SourceSelector = sourceSelector;
    }
}

