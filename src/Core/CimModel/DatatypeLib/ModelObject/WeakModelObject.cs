using System.Collections.Concurrent;
using CimBios.Core.CimModel.CimDatatypeLib.EventUtils;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.CimDatatypeLib.Utils;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.Schema.AutoSchema;

namespace CimBios.Core.CimModel.CimDatatypeLib;

/// <summary>
///     Weak linked model object. Provides meta schema validation free IO functions.
///     Use only to represent data in unknown schema format. Meta class always auto.
/// </summary>
public class WeakModelObject : DynamicModelObjectBase,
    IModelObject, IStatementsContainer
{
    private readonly CimAutoClass _MetaClass;

    private readonly IOIDDescriptor _Oid;

    protected readonly Dictionary<ICimMetaProperty, object?>
        _PropertiesData = [];

    protected readonly
        Dictionary<ICimMetaProperty, ICollection<IModelObject>>
        _Statements = [];

    /// <summary>
    ///     Default weak linked object constructor.
    /// </summary>
    public WeakModelObject(IOIDDescriptor oid,
        ICimMetaClass metaClass, bool initializeProperties = false)
    {
        if (metaClass is not CimAutoClass autoClass)
        {
            autoClass = new CimAutoClass(
                metaClass.BaseUri,
                metaClass.ShortName,
                metaClass.Description
            );

            autoClass.SetIsCompound(metaClass.IsCompound);
            autoClass.SetIsAbstract(metaClass.IsAbstract);
            autoClass.SetIsEnum(metaClass.IsEnum);
        }

        if (initializeProperties)
            foreach (var metaProperty in metaClass.AllProperties)
                autoClass.AddProperty(metaProperty);

        _Oid = oid;
        _MetaClass = autoClass;

        InitStatementsCollections();
    }

    /// <summary>
    ///     Copy constuctor.
    /// </summary>
    /// <param name="modelObject">Model object for copy.</param>
    public WeakModelObject(IReadOnlyModelObject modelObject)
        : this(modelObject.OID, modelObject.MetaClass, true)
    {
        this.CopyPropertiesFrom(modelObject, true);
    }

    public override IOIDDescriptor OID => _Oid;
    public override ICimMetaClass MetaClass => _MetaClass;

    public override bool HasProperty(string propertyName)
    {
        var metaProperty = TryGetMetaPropertyByName(propertyName);
        if (metaProperty != null) return _PropertiesData.ContainsKey(metaProperty);

        return false;
    }

    public override object? GetAttribute(ICimMetaProperty metaProperty)
    {
        if (metaProperty.PropertyKind == CimMetaPropertyKind.Attribute
            && _PropertiesData.TryGetValue(metaProperty, out var value))
            return value;

        return null;
    }

    public override object? GetAttribute(string attributeName)
    {
        var metaProperty = TryGetMetaPropertyByName(attributeName);
        if (metaProperty != null) return GetAttribute(metaProperty);

        return null;
    }

    public override T? GetAttribute<T>(ICimMetaProperty metaProperty)
        where T : default
    {
        var data = GetAttribute(metaProperty);
        if (data is T typedValue) return typedValue;

        if (data is EnumValueObject enumValueObject) return (T)enumValueObject.AsEnum();

        return default;
    }

    public override T? GetAttribute<T>(string attributeName)
        where T : default
    {
        var metaProperty = TryGetMetaPropertyByName(attributeName);
        if (metaProperty != null) return GetAttribute<T>(metaProperty);

        return default;
    }

    public override void SetAttribute<T>(ICimMetaProperty metaProperty,
        T? value) where T : default
    {
        if (value is Enum enumValue)
        {
            this.SetAttributeAsEnum(metaProperty, enumValue);
            return;
        }

        if (metaProperty.PropertyKind != CimMetaPropertyKind.Attribute)
            throw new ArgumentException(
                $"Property {metaProperty.ShortName} is not attribute!");

        if (value is not string
            && value != null
            && !value.GetType().IsPrimitive
            && !typeof(T).IsAssignableFrom(typeof(IModelObject))
            && value is not EnumValueObject)
            throw new ArgumentException(
                $"Attribute {metaProperty.ShortName} can not be assigning by value of type {typeof(T).Name}!");

        if (CanChangeProperty(metaProperty, value) == false) return;

        if (_PropertiesData.TryGetValue(metaProperty, out var old))
        {
            if (value == null && old == null || old?.Equals(value) == true)
            {
                return;
            }
            
            _PropertiesData[metaProperty] = value;
            
            if (value is IModelObject compound)
            {
                SubscribeToCompoundChanges(metaProperty, compound);
            }
        }
        else
        {
            _PropertiesData.TryAdd(metaProperty, value);
            _MetaClass.AddProperty(metaProperty);
            
            if (value is IModelObject compound)
            {
                SubscribeToCompoundChanges(metaProperty, compound);
            }
        }

        if (_MetaClass.HasProperty(metaProperty) == false) 
            _MetaClass.AddProperty(metaProperty);

        CleanUpNullProperty(metaProperty);

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

        SetAttribute(metaProperty, value);
    }

    public override IModelObject InitializeCompoundAttribute(
        ICimMetaProperty metaProperty, bool reset = true)
    {
        if (metaProperty.PropertyDatatype == null
            || metaProperty.PropertyDatatype.IsCompound == false)
            throw new NotSupportedException(
                $"Undefined compound property {metaProperty.ShortName} class type!");

        if (GetAttribute(metaProperty) is IModelObject currentValue
            && reset == false)
            return currentValue;

        IModelObject? compoundObject = null;

        if (InternalTypeLib != null)
            compoundObject = InternalTypeLib.CreateCompoundInstance(
                new WeakModelObjectFactory(),
                metaProperty.PropertyDatatype);
        else
            compoundObject = new WeakModelObject(new AutoDescriptor(),
                metaProperty.PropertyDatatype, true);

        if (compoundObject != null)
        {
            SetAttribute(metaProperty, compoundObject);
            return compoundObject;
        }

        throw new NotSupportedException(
            $"Weak object error while {metaProperty.ShortName} compound object creation!");
    }

    public override IModelObject InitializeCompoundAttribute(
        string attributeName, bool reset = true)
    {
        const string AutoCompoundClassName = "_AutoCompound";

        var compoundMetaClass = new CimAutoClass(
            new Uri(CimAutoSchemaSerializer.BaseSchemaUri
                    + "#" + AutoCompoundClassName),
            AutoCompoundClassName, string.Empty);

        compoundMetaClass.SetIsCompound(true);

        return InitializeCompoundAttribute(attributeName,
            compoundMetaClass, reset);
    }

    public override T? GetAssoc1To1<T>(ICimMetaProperty metaProperty)
        where T : default
    {
        if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1
            && _PropertiesData.TryGetValue(metaProperty, out var value)
            && value is ICollection<T> tObjCol)
            return tObjCol.FirstOrDefault();

        return default;
    }

    public override T? GetAssoc1To1<T>(string assocName) where T : default
    {
        var metaProperty = TryGetMetaPropertyByName(assocName);
        if (metaProperty != null) return GetAssoc1To1<T>(metaProperty);

        return default;
    }

    /// <summary>
    ///     Note: inverse association does not assigning!
    /// </summary>
    public override void SetAssoc1To1(ICimMetaProperty metaProperty,
        IModelObject? obj)
    {
        if (metaProperty.PropertyKind != CimMetaPropertyKind.Assoc1To1
            && metaProperty.PropertyKind != CimMetaPropertyKind.Assoc1ToM)
            throw new ArgumentException(
                $"Property {metaProperty.ShortName} is not association!");

        RemoveAllAssocs1ToM(metaProperty);

        if (obj != null) AddAssoc1ToM(metaProperty, obj);

        CleanUpNullProperty(metaProperty);
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
            return collection.ToArray();

        return [];
    }

    public override IModelObject[] GetAssoc1ToM(string assocName)
    {
        var metaProperty = TryGetMetaPropertyByName(assocName);
        if (metaProperty != null) return GetAssoc1ToM(metaProperty);

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

    public override void Shrink()
    {
        _PropertiesData.TrimExcess();
    }

    public override void AddAssoc1ToM(ICimMetaProperty metaProperty,
        IModelObject obj)
    {
        if (metaProperty.PropertyKind != CimMetaPropertyKind.Assoc1To1
            && metaProperty.PropertyKind != CimMetaPropertyKind.Assoc1ToM)
            throw new ArgumentException(
                $"Property {metaProperty.ShortName} is not association!");

        if (CanChangeProperty(metaProperty, obj) == false) return;

        if (_PropertiesData.TryGetValue(metaProperty, out var data)
            && data is HashSet<IModelObject> dataCollection)
        {
            if (dataCollection.Contains(obj)) return;

            dataCollection.Add(obj);
        }
        else
        {
            _PropertiesData.TryAdd(metaProperty,
                new HashSet<IModelObject>(
                    new ModelObjectOIDEqualityComparer()) { obj });

            _MetaClass.AddProperty(metaProperty);
        }

        if (_MetaClass.HasProperty(metaProperty) == false) _MetaClass.AddProperty(metaProperty);

        CleanUpNullProperty(metaProperty);

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
        if (metaProperty.PropertyKind != CimMetaPropertyKind.Assoc1To1
            && metaProperty.PropertyKind != CimMetaPropertyKind.Assoc1ToM)
            throw new ArgumentException(
                $"Property {metaProperty.ShortName} is not association!");

        if (CanChangeProperty(metaProperty, obj, true) == false) return;

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
        if (metaProperty == null) return;

        RemoveAssoc1ToM(assocName, obj);
    }

    public override void RemoveAllAssocs1ToM(ICimMetaProperty metaProperty)
    {
        if (metaProperty.PropertyKind != CimMetaPropertyKind.Assoc1To1
            && metaProperty.PropertyKind != CimMetaPropertyKind.Assoc1ToM)
            throw new ArgumentException(
                $"Property {metaProperty.ShortName} is not association!");

        if (_PropertiesData.TryGetValue(metaProperty, out var data)
            && data is ICollection<IModelObject> dataCollection)
            foreach (var assocObject in dataCollection)
                RemoveAssoc1ToM(metaProperty, assocObject);
    }

    public override void RemoveAllAssocs1ToM(string assocName)
    {
        var metaProperty = TryGetMetaPropertyByName(assocName);
        if (metaProperty == null) return;

        RemoveAllAssocs1ToM(assocName);
    }


    public IReadOnlyDictionary<ICimMetaProperty, ICollection<IModelObject>>
        Statements => _Statements.AsReadOnly();

    public void AddToStatements(ICimMetaProperty statementProperty,
        IModelObject statement)
    {
        if (statementProperty.PropertyKind != CimMetaPropertyKind.Statements)
            throw new ArgumentException(
                $"Property {statementProperty.ShortName} is not statement!");

        if (_Statements.TryGetValue(statementProperty,
                out var statements) == false)
            _Statements.TryAdd(statementProperty,
                new HashSet<IModelObject> { statement });
        else if (statements.Contains(statement) == false) statements.Add(statement);
    }

    public void RemoveFromStatements(ICimMetaProperty statementProperty,
        IModelObject statement)
    {
        if (_Statements.TryGetValue(statementProperty,
                out var statements))
            statements.Remove(statement);
    }

    public IModelObject InitializeCompoundAttribute(
        string attributeName, ICimMetaClass compoundMetaClass, bool reset = true)
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

            newAutoProperty.SetPropertyDatatype(compoundMetaClass);
            newAutoProperty.SetPropertyKind(CimMetaPropertyKind.Attribute);
            metaProperty = newAutoProperty;
        }

        return InitializeCompoundAttribute(metaProperty, reset);
    }

    private void InitStatementsCollections()
    {
        foreach (var statementProperty in MetaClass.AllProperties
                     .Where(p => p.PropertyKind == CimMetaPropertyKind.Statements))
            _Statements.TryAdd(statementProperty,
                new HashSet<IModelObject>());
    }

    private void CleanUpNullProperty(ICimMetaProperty metaProperty)
    {
        if (_PropertiesData.TryGetValue(metaProperty, out var data) == false) return;

        if ((metaProperty.PropertyKind == CimMetaPropertyKind.Attribute
             || metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1)
            && data == null)
            _PropertiesData.Remove(metaProperty, out _);
        else if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM
                 && data is ICollection<IModelObject> dataCol
                 && dataCol.Count == 0)
            _PropertiesData.Remove(metaProperty, out _);
    }
}

public class WeakModelObjectFactory : IModelObjectFactory
{
    public Type ProduceType => typeof(WeakModelObject);

    public IModelObject Create(IOIDDescriptor oid,
        ICimMetaClass metaClass)
    {
        if (metaClass is not CimMetaClassBase metaClassBase) throw new InvalidCastException();

        return new WeakModelObject(oid, metaClass);
    }
}