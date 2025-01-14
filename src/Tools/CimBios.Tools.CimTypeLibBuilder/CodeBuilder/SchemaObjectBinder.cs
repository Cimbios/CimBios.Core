using CimBios.Core.CimModel.Schema;

namespace CimBios.Tools.CimTypeLibBuilder.CodeBuilder;

public interface ISchemaObjectBinder
{
    public ICimMetaResource? MappedObject { get; set; }
    public string GetVariableValue(string name);
}

public class NamespaceBinder : ISchemaObjectBinder
{
    public ICimMetaResource? MappedObject { get; set; }

    public string ns { get; set; } = string.Empty;

    public NamespaceBinder()
    {
        _propertyMap.Add(nameof(ns), () => ns);
    }

    public string GetVariableValue(string name)
    {
        return _propertyMap[name].Invoke();
    }

    private readonly Dictionary<string, Func<string>> _propertyMap = [];
}

public class AnnotationBinder : ISchemaObjectBinder
{
    public ICimMetaResource? MappedObject { get; set; }

    public string annotation_text => MappedObject!.Description;

    public AnnotationBinder()
    {
        _propertyMap.Add(nameof(annotation_text), () => annotation_text);
    }

    public string GetVariableValue(string name)
    {
        return _propertyMap[name].Invoke();
    }

    private readonly Dictionary<string, Func<string>> _propertyMap = [];
}

public class ClassBinder : ISchemaObjectBinder
{
    public ICimMetaResource? MappedObject { get; set; }

    private ICimMetaClass cimMetaClass 
    {
        get
        {
            if (MappedObject is not ICimMetaClass metaClass)
            {
                throw new Exception("invalid casting of metaClass");
            }

            return metaClass;
        }
    }

    public string class_uri => cimMetaClass.BaseUri.AbsoluteUri;
    public string class_name => cimMetaClass.ShortName;
    public string parent_class => cimMetaClass.ParentClass == null 
        ? "ModelObject" : cimMetaClass.ParentClass.ShortName;

    public ClassBinder()
    {
        _propertyMap.Add(nameof(class_uri), () => class_uri);
        _propertyMap.Add(nameof(class_name), () => class_name);
        _propertyMap.Add(nameof(parent_class), () => parent_class);
    }

    public string GetVariableValue(string name)
    {
        return _propertyMap[name].Invoke();
    }

    private readonly Dictionary<string, Func<string>> _propertyMap = [];
}

public class EnumBinder : ISchemaObjectBinder
{
    public ICimMetaResource? MappedObject { get; set; }

    private ICimMetaClass cimMetaClass 
    {
        get
        {
            if (MappedObject is not ICimMetaClass metaClass
                || metaClass.IsEnum == false)
            {
                throw new Exception("invalid casting of metaClass (enum)");
            }

            return metaClass;
        }
    }

    public string enum_uri => cimMetaClass.BaseUri.AbsoluteUri;
    public string enum_name => cimMetaClass.ShortName;

    public EnumBinder()
    {
        _propertyMap.Add(nameof(enum_uri), () => enum_uri);
        _propertyMap.Add(nameof(enum_name), () => enum_name);
    }

    public string GetVariableValue(string name)
    {
        return _propertyMap[name].Invoke();
    }

    private readonly Dictionary<string, Func<string>> _propertyMap = [];
}

public class EnumValueBinder : ISchemaObjectBinder
{
    public ICimMetaResource? MappedObject { get; set; }

    private ICimMetaIndividual cimMetaIndividual 
    {
        get
        {
            if (MappedObject is not ICimMetaIndividual cimMetaIndividual)
            {
                throw new Exception("invalid casting of MetaIndividual (enum)");
            }

            return cimMetaIndividual;
        }
    }

    public string enum_value_name => cimMetaIndividual.ShortName;

    public EnumValueBinder()
    {
        _propertyMap.Add(nameof(enum_value_name), () => enum_value_name);
    }

    public string GetVariableValue(string name)
    {
        return _propertyMap[name].Invoke();
    }

    private readonly Dictionary<string, Func<string>> _propertyMap = [];
}

public class PropertyBinder : ISchemaObjectBinder
{
    public ICimMetaResource? MappedObject { get; set; }

    private ICimMetaProperty cimMetaProperty 
    {
        get
        {
            if (MappedObject is not ICimMetaProperty metaProperty)
            {
                throw new Exception("invalid casting of metaProperty");
            }

            return metaProperty;
        }
    }

    public string property_name => cimMetaProperty.ShortName;
    public string datatype
    {
        get
        {
            if (cimMetaProperty.PropertyDatatype is ICimMetaDatatype metaType)
            {
                return metaType.PrimitiveType.Name;
            }
            else if (cimMetaProperty.PropertyDatatype != null)
            {
                return cimMetaProperty.PropertyDatatype.ShortName;
            }

            return "object";
        }
    }

    public PropertyBinder()
    {
        _propertyMap.Add(nameof(property_name), () => property_name);
        _propertyMap.Add(nameof(datatype), () => datatype);
    }

    public string GetVariableValue(string name)
    {
        return _propertyMap[name].Invoke();
    }

    private readonly Dictionary<string, Func<string>> _propertyMap = [];
}

