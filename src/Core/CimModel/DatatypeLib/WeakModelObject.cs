using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.Schema.AutoSchema;

namespace CimBios.Core.CimModel.DatatypeLib;

public class WeakModelObject : DynamicModelObjectBase, IModelObject
{
    public override string Uuid => _Uuid;

    public override ICimMetaClass MetaClass => _MetaClass;

    public override bool IsAuto => _IsAuto;

    public WeakModelObject(string uuid, CimAutoClass metaClass, bool isAuto)
        : base()
    {
        _Uuid = uuid;
        _MetaClass = metaClass;
        _IsAuto = isAuto;
    }

    public override bool HasProperty(string propertyName)
    {
        var metaProperty = TryGetMetaPropertyByName(propertyName);
        if (metaProperty != null)
        {
            return _PropertiesData.ContainsKey(metaProperty);
        }

        return false;
    }

    public override object? GetAttribute(ICimMetaProperty metaProperty)
    {
        if (metaProperty.PropertyKind == CimMetaPropertyKind.Attribute
            &&_PropertiesData.TryGetValue(metaProperty, out var value))
        {
            return value;
        }

        return null;
    }

    public override object? GetAttribute(string attributeName)
    {
        var metaProperty = TryGetMetaPropertyByName(attributeName);
        if (metaProperty != null)
        {
            return GetAttribute(metaProperty);
        }

        throw new ArgumentException(
            $"No such meta property with name {attributeName}!");
    }

    public override T? GetAttribute<T>(ICimMetaProperty metaProperty) 
        where T : default
    {
        if (GetAttribute(metaProperty) is T typedValue)
        {
            return typedValue;
        }

        return default;
    }

    public override T? GetAttribute<T>(string attributeName) 
        where T : default
    {
        var metaProperty = TryGetMetaPropertyByName(attributeName);
        if (metaProperty != null)
        {
            return GetAttribute<T>(metaProperty);
        }

        throw new ArgumentException(
            $"No such meta property with name {attributeName}!");
    }

    public override void SetAttribute<T>(ICimMetaProperty metaProperty, 
        T? value) where T : default
    {
        if (metaProperty.PropertyKind != CimMetaPropertyKind.Attribute)
        {
            throw new ArgumentException(
                $"Attribute {metaProperty.ShortName} does not exist!");
        }

        if (value is not string 
            && !typeof(T).IsAssignableTo(typeof(IModelObject)))
        {
            throw new ArgumentException(
                $"Attribute {metaProperty.ShortName} can not be assigning by value of type {typeof(T).Name}!");
        }

        if (CanChangeProperty(metaProperty) == false)
        {
            return;
        }

        if (_PropertiesData.ContainsKey(metaProperty))
        {
            _PropertiesData[metaProperty] = value;
        }
        else
        {
            _PropertiesData.Add(metaProperty, value);
            _MetaClass.AddProperty(metaProperty);
        }

        OnPropertyChanged(new 
            CimMetaPropertyChangedEventArgs(metaProperty));
    }

    public override void SetAttribute<T>(string attributeName, 
        T? value) where T : default
    {
        var metaProperty = TryGetMetaPropertyByName(attributeName);
        if (metaProperty == null)
        {
            metaProperty = new CimAutoProperty()
            {
                BaseUri = new Uri(CimAutoSchemaSerializer.BaseSchemaUri 
                    + "#" + attributeName),
                ShortName = attributeName,
                Description = string.Empty,
                PropertyKind = CimMetaPropertyKind.Attribute
            };
        }

        SetAttribute<T>(metaProperty, value);
    }

    public override T? GetAssoc1To1<T>(ICimMetaProperty metaProperty) 
        where T : default
    {
        if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1
            &&_PropertiesData.TryGetValue(metaProperty, out var value)
            && value is T tObj)
        {
            return tObj;
        }

        return default;
    }

    public override T? GetAssoc1To1<T>(string assocName) where T : default
    {
        var metaProperty = TryGetMetaPropertyByName(assocName);
        if (metaProperty != null)
        {
            return GetAssoc1To1<T>(metaProperty);
        }

        throw new ArgumentException(
            $"No such meta property with name {assocName}!");
    }

    /// <summary>
    /// Note: inverse association does not assigning!
    /// </summary>
    public override void SetAssoc1To1(ICimMetaProperty metaProperty, 
        IModelObject? obj)
    {
        if (metaProperty.PropertyKind != CimMetaPropertyKind.Assoc1To1)
        {
            throw new ArgumentException(
                $"Attribute {metaProperty.ShortName} does not exist!");
        }

        if (CanChangeProperty(metaProperty) == false)
        {
            return;
        }

        if (_PropertiesData.ContainsKey(metaProperty))
        {
            _PropertiesData[metaProperty] = obj;
        }
        else
        {
            _PropertiesData.Add(metaProperty, obj);
            _MetaClass.AddProperty(metaProperty);
        }

        OnPropertyChanged(new 
            CimMetaPropertyChangedEventArgs(metaProperty));
    }

    public override void SetAssoc1To1(string assocName, IModelObject? obj)
    {
        var metaProperty = TryGetMetaPropertyByName(assocName);
        if (metaProperty == null)
        {
            metaProperty = new CimAutoProperty()
            {
                BaseUri = new Uri(CimAutoSchemaSerializer.BaseSchemaUri 
                    + "#" + assocName),
                ShortName = assocName,
                Description = string.Empty,
                PropertyKind = CimMetaPropertyKind.Assoc1To1,
            };
        }

        SetAssoc1To1(metaProperty, obj);
    }

    public override IModelObject[] GetAssoc1ToM(ICimMetaProperty metaProperty)
    {
        throw new NotImplementedException();
    }

    public override IModelObject[] GetAssoc1ToM(string assocName)
    {
        throw new NotImplementedException();
    }

    public override T[] GetAssoc1ToM<T>(ICimMetaProperty metaProperty)
    {
        throw new NotImplementedException();
    }

    public override T[] GetAssoc1ToM<T>(string assocName)
    {
        throw new NotImplementedException();
    }

    public override void AddAssoc1ToM(ICimMetaProperty metaProperty, 
        IModelObject obj)
    {
        throw new NotImplementedException();
    }

    public override void AddAssoc1ToM(string assocName, IModelObject obj)
    {
        throw new NotImplementedException();
    }

    public override void RemoveAllAssocs1ToM(ICimMetaProperty metaProperty)
    {
        throw new NotImplementedException();
    }

    public override void RemoveAllAssocs1ToM(string assocName)
    {
        throw new NotImplementedException();
    }

    public override void RemoveAssoc1ToM(ICimMetaProperty metaProperty, 
        IModelObject obj)
    {
        throw new NotImplementedException();
    }

    public override void RemoveAssoc1ToM(string assocName, IModelObject obj)
    {
        throw new NotImplementedException();
    }

    private string _Uuid;
    private CimAutoClass _MetaClass;
    private bool _IsAuto;

    private readonly Dictionary<ICimMetaProperty, object?> _PropertiesData = [];

}

public class WeakModelObjectFactory : IModelObjectFactory
{
    public System.Type ProduceType => typeof(WeakModelObject);

    public IModelObject Create(string uuid, 
        ICimMetaClass metaClass, bool isAuto)
    {
        if (metaClass is not CimAutoClass autoMetaClass)
        {
            throw new InvalidCastException();
        }

        return new WeakModelObject(uuid, autoMetaClass, isAuto);
    }
}
