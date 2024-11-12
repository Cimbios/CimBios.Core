namespace CimBios.Utils.MetaReflectionHelper;

public class MetaTypeAttribute : Attribute
{
    public string Identifier { get; }

    public MetaTypeAttribute(string identifier)
    {
        Identifier = identifier;
    }
}
