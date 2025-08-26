namespace CimBios.Utils.MetaReflectionHelper;

public class MetaTypeAttribute : Attribute
{
    public MetaTypeAttribute(string identifier)
    {
        Identifier = identifier;
    }

    public string Identifier { get; }
}