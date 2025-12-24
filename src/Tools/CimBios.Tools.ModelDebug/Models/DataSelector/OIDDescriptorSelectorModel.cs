using CimBios.Core.CimModel.DatatypeLib.OID;

namespace CimBios.Tools.ModelDebug.Models.DataSelector;

public class OIDDescriptorSelectorModel(
    string title,
    IOIDDescriptorFactory oidDescriptorFactoryFactory)
{
    public string Title { get; } = title;

    public IOIDDescriptorFactory OIDDescriptorFactory { get; }
        = oidDescriptorFactoryFactory;
}