using CimBios.Core.CimModel.CimDatatypeLib.OID;

namespace CimBios.Tools.ModelDebug.Models;

public class OIDDescriptorSelectorModel(
    string title,
    IOIDDescriptorFactory oidDescriptorFactoryFactory)
{
    public string Title { get; } = title;

    public IOIDDescriptorFactory OIDDescriptorFactory { get; }
        = oidDescriptorFactoryFactory;
}