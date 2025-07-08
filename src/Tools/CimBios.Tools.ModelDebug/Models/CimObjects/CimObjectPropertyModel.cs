namespace CimBios.Tools.ModelDebug.Models;

public class CimObjectPropertyModel : TreeViewNodeModel
{
    private string _Name = "(noname)";
    private string _Value = "(novalue)";

    public string Name
    {
        get => _Name;
        set
        {
            _Name = value;
            OnPropertyChanged();
        }
    }

    public string Value
    {
        get => _Value;
        set
        {
            _Value = value;
            OnPropertyChanged();
        }
    }
}