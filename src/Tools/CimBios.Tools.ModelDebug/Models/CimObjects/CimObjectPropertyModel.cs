using CommunityToolkit.Mvvm.ComponentModel;

namespace CimBios.Tools.ModelDebug.Models;

public class CimObjectPropertyModel : TreeViewNodeModel
{
    public string Name
    {
        get => _Name;
        set 
        {
            _Name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    public string Value
    {
        get => _Value;
        set 
        {
            _Value = value;
            OnPropertyChanged(nameof(Value));
        }
    }

    private string _Name = "(noname)";
    private string _Value = "(novalue)";
}
