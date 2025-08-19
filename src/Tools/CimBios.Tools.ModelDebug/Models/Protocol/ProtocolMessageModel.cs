using CimBios.Tools.ModelDebug.Services;

namespace CimBios.Tools.ModelDebug.Models;

public class ProtocolMessageModel : TreeViewNodeModel
{
    public ProtocolMessageModel(ProtocolMessage message)
    {
        Message = message;

        Title = message.Text;
    }

    public ProtocolMessage Message { get; }
}