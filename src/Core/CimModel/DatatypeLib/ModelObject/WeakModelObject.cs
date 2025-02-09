using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.Schema.AutoSchema;

namespace CimBios.Core.CimModel.DatatypeLib;

public class WeakModelObject : DynamicModelObjectBase, 
    IModelObject, IStatementsContainer
{
    public override string OID => _Oid;

    public override ICimMetaClass MetaClass => _MetaClass;

    public override bool IsAuto => _IsAuto;

    public IReadOnlyDictionary<ICimMetaProperty, ICollection<IModelObject>> 
    Statements => _Statements.AsReadOnly();

    internal WeakModelObject(string oid, CimMetaClassBase metaClass, bool isAuto)
        : base()
    {
        _Oid = oid;
        _MetaClass = metaClass;
        _IsAuto = isAuto;

        InitStatementsCollections();
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
            && _PropertiesData.TryGetValue(metaProperty, out var value))
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

        return null;
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

        return default;
    }

    public override void SetAttribute<T>(ICimMetaProperty metaProperty, 
        T? value) where T : default
    {
        if (metaProperty.PropertyKind != CimMetaPropertyKind.Attribute)
        {
            throw new ArgumentException(
                $"Property {metaProperty.ShortName} is not attribute!");
        }

        if (value is not string 
            && value != null
            && !value.GetType().IsPrimitive
            && !typeof(T).IsAssignableFrom(typeof(IModelObject)))
        {
            throw new ArgumentException(
                $"Attribute {metaProperty.ShortName} can not be assigning by value of type {typeof(T).Name}!");
        }

        if (CanChangeProperty(metaProperty, value) == false)
        {
            return;
        }

        object? old = null;
        if (_PropertiesData.ContainsKey(metaProperty))
        {
            old = _PropertiesData[metaProperty];
            _PropertiesData[metaProperty] = value;
        }
        else
        {
            _PropertiesData.Add(metaProperty, value);
            _MetaClass.AddProperty(metaProperty);
        }

        if (_MetaClass.HasProperty(metaProperty) == false)
        {
            _MetaClass.AddProperty(metaProperty);
        }

        OnPropertyChanged(new CimMetaAttributeChangedEventArgs(
            metaProperty, old, value));
    }

    public override void SetAttribute<T>(string attributeName, 
        T? value) where T : default
    {
        var metaProperty = TryGetMetaPropertyByName(attributeName);
        if (metaProperty == null)
        {
            var newAutoProperty = new CimAutoProperty(
                new Uri(CimAutoSchemaSerializer.BaseSchemaUri 
                    + "#" + attributeName),
                attributeName.Split('.').Last(),
                string.Empty
            );
            newAutoProperty.SetPropertyKind(CimMetaPropertyKind.Attribute);
            metaProperty = newAutoProperty;
        }

        SetAttribute<T>(metaProperty, value);
    }

    public override T? GetAssoc1To1<T>(ICimMetaProperty metaProperty) 
        where T : default
    {
        if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1
            && _PropertiesData.TryGetValue(metaProperty, out var value)
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

        return default;
    }

    /// <summary>
    /// Note: inverse association does not assigning!
    /// </summary>
    public override void SetAssoc1To1(ICimMetaProperty metaProperty, 
        IModelObject? obj)
    {
        if (metaProperty.PropertyKind != CimMetaPropertyKind.Assoc1To1
            && metaProperty.PropertyKind != CimMetaPropertyKind.Assoc1ToM)
        {
            throw new ArgumentException(
                $"Property {metaProperty.ShortName} is not association!");
        }

        RemoveAllAssocs1ToM(metaProperty);

        if (obj != null)
        {
            AddAssoc1ToM(metaProperty, obj);
        }
    }

    public override void SetAssoc1To1(string assocName, IModelObject? obj)
    {
        var metaProperty = TryGetMetaPropertyByName(assocName);
        if (metaProperty == null)
        {
            var newAutoProperty = new CimAutoProperty(
                new Uri(CimAutoSchemaSerializer.BaseSchemaUri 
                    + "#" + assocName),
                assocName.Split('.').Last(),
                string.Empty
            );
            newAutoProperty.SetPropertyKind(CimMetaPropertyKind.Assoc1ToM);
            metaProperty = newAutoProperty;
        }
        
        SetAssoc1To1(metaProperty, obj);
    }

    public override IModelObject[] GetAssoc1ToM(ICimMetaProperty metaProperty)
    {
        if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM
            && _PropertiesData.TryGetValue(metaProperty, out var value)
            && value is ICollection<IModelObject> collection)
        {
            return collection.ToArray();
        }

        return [];
    }

    public override IModelObject[] GetAssoc1ToM(string assocName)
    {
        var metaProperty = TryGetMetaPropertyByName(assocName);
        if (metaProperty != null)
        {
            return GetAssoc1ToM(metaProperty);
        }

        return [];
    }

    public override T[] GetAssoc1ToM<T>(ICimMetaProperty metaProperty)
    {
        return GetAssoc1ToM(metaProperty).Cast<T>().ToArray();
    }

    public override T[] GetAssoc1ToM<T>(string assocName)
    {
        return GetAssoc1ToM(assocName).Cast<T>().ToArray();
    }

    public override void AddAssoc1ToM(ICimMetaProperty metaProperty, 
        IModelObject obj)
    {
        if (metaProperty.PropertyKind != CimMetaPropertyKind.Assoc1ToM)
        {
            throw new ArgumentException(
                $"Property {metaProperty.ShortName} is not association!");
        }

        if (CanChangeProperty(metaProperty, obj) == false)
        {
            return;
        }

        if (_PropertiesData.TryGetValue(metaProperty, out var data)
            && data is ICollection<IModelObject> dataCollection) 
        {
            dataCollection.Add(obj);
        }
        else
        {
            _PropertiesData.Add(metaProperty, 
                new HashSet<IModelObject>() { obj });

            _MetaClass.AddProperty(metaProperty);
        }

        if (_MetaClass.HasProperty(metaProperty) == false)
        {
            _MetaClass.AddProperty(metaProperty);
        }

        OnPropertyChanged(new CimMetaAssocChangedEventArgs(
            metaProperty, null, obj));
    }

    public override void AddAssoc1ToM(string assocName, IModelObject obj)
    {
        var metaProperty = TryGetMetaPropertyByName(assocName);
        if (metaProperty == null)
        {
            var newAutoProperty = new CimAutoProperty(
                new Uri(CimAutoSchemaSerializer.BaseSchemaUri 
                    + "#" + assocName),
                assocName.Split('.').Last(),
                string.Empty
            );
            newAutoProperty.SetPropertyKind(CimMetaPropertyKind.Assoc1ToM);
            metaProperty = newAutoProperty;
        }
        
        AddAssoc1ToM(metaProperty, obj);
    }

    public override void RemoveAssoc1ToM(ICimMetaProperty metaProperty, 
        IModelObject obj)
    {
        if (metaProperty.PropertyKind != CimMetaPropertyKind.Assoc1ToM)
        {
            throw new ArgumentException(
                $"Property {metaProperty.ShortName} is not association!");
        }

        if (CanChangeProperty(metaProperty, obj, true) == false)
        {
            return;
        }

        if (_PropertiesData.TryGetValue(metaProperty, out var data)
            && data is ICollection<IModelObject> dataCollection
            && dataCollection.Contains(obj)) 
        {
            dataCollection.Remove(obj);
        
            OnPropertyChanged(new CimMetaAssocChangedEventArgs(
                metaProperty, obj, null));
        }
    }

    public override void RemoveAssoc1ToM(string assocName, IModelObject obj)
    {
        var metaProperty = TryGetMetaPropertyByName(assocName);
        if (metaProperty == null)
        {
            return;
        }

        RemoveAssoc1ToM(assocName, obj);
    }

    public override void RemoveAllAssocs1ToM(ICimMetaProperty metaProperty)
    {
        if (metaProperty.PropertyKind != CimMetaPropertyKind.Assoc1ToM)
        {
            throw new ArgumentException(
                $"Property {metaProperty.ShortName} is not association!");
        }

        if (_PropertiesData.TryGetValue(metaProperty, out var data)
            && data is ICollection<IModelObject> dataCollection) 
        {
            foreach (var assocObject in dataCollection)
            {
                RemoveAssoc1ToM(metaProperty, assocObject);
            }
        }
    }

    public override void RemoveAllAssocs1ToM(string assocName)
    {
        var metaProperty = TryGetMetaPropertyByName(assocName);
        if (metaProperty == null)
        {
            return;
        }

        RemoveAllAssocs1ToM(assocName);
    }

    public void AddToStatements(ICimMetaProperty statementProperty,
        IModelObject statement)
    {
        if (statementProperty.PropertyKind != CimMetaPropertyKind.Statements)
        {
            throw new ArgumentException(
                $"Property {statementProperty.ShortName} is not statement!");
        }

        if (_Statements.TryGetValue(statementProperty, 
            out var statements) == false)
        {
            _Statements.Add(statementProperty, 
                new HashSet<IModelObject>() { statement });
        }
        else if (statements.Contains(statement) == false)
        {
            statements.Add(statement);
        }
    }

    public void RemoveFromStatements(ICimMetaProperty statementProperty,
        IModelObject statement)
    {
        if (_Statements.TryGetValue(statementProperty, 
            out var statements))
        {
            statements.Remove(statement);
        }
    }

    private void InitStatementsCollections()
    {
        foreach (var statementProperty in MetaClass.AllProperties
            .Where(p => p.PropertyKind == CimMetaPropertyKind.Statements))
        {
            _Statements.Add(statementProperty, 
                new HashSet<IModelObject>());
        }
    }

    private string _Oid;
    private CimMetaClassBase _MetaClass;
    private bool _IsAuto;

    private readonly Dictionary<ICimMetaProperty, object?> _PropertiesData = [];

    private readonly Dictionary<ICimMetaProperty, ICollection<IModelObject>> 
    _Statements = [];
}

public class WeakModelObjectFactory : IModelObjectFactory
{
    public System.Type ProduceType => typeof(WeakModelObject);

    public IModelObject Create(string uuid, 
        ICimMetaClass metaClass, bool isAuto)
    {
        if (metaClass is not CimMetaClassBase metaClassBase)
        {
            throw new InvalidCastException();
        }

        return new WeakModelObject(uuid, metaClassBase, isAuto);
    }
}
