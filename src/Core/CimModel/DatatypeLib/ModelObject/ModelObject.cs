using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.CimDatatypeLib.EventUtils;
using System.Collections.Concurrent;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.CimDatatypeLib.Utils;

namespace CimBios.Core.CimModel.CimDatatypeLib;

/// <summary>
/// Implementation of CIM meta typed identified object.
/// </summary>
public class ModelObject : DynamicModelObjectBase, 
    IModelObject, IStatementsContainer
{
    public override IOIDDescriptor OID { get; }

    public override ICimMetaClass MetaClass { get; }

    public IReadOnlyDictionary<ICimMetaProperty, ICollection<IModelObject>> 
    Statements => _Statements.AsReadOnly();

    public ModelObject(IOIDDescriptor oid, ICimMetaClass metaClass)
    {
        OID = oid;

        MetaClass = metaClass;
        _PropertiesData = [];

        //InitStatementsCollections();
    }

    public override bool HasProperty(string propertyName)
    {
        var metaProperty = TryGetMetaPropertyByName(propertyName);
        if (metaProperty != null)
        {
            return MetaClass.AllProperties.Contains(metaProperty);
        }

        return false;
    }

    #region AttributesLogic

    public override object? GetAttribute(ICimMetaProperty metaProperty)
    {
        var data = GetDataByProperty(metaProperty, 
            CimMetaPropertyKind.Attribute);

        return data;
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
        where T: default
    {
        var value = GetAttribute(metaProperty);
        if (value is T typedValue)
        {
            return typedValue;
        }
        else if (value is EnumValueObject enumValueObject)
        {
            return (T)enumValueObject.AsEnum();
        }

        return default;
    }

    public override T? GetAttribute<T>(string attributeName) 
        where T: default
    {
        var metaProperty = TryGetMetaPropertyByName(attributeName);
        if (metaProperty != null)
        {
            return GetAttribute<T>(metaProperty);
        }

        throw new ArgumentException(
            $"No such meta property with name {attributeName}!");
    }

    public override void SetAttribute<T>(ICimMetaProperty metaProperty, T? value) 
        where T: default
    {
        if (value is Enum enumValue)
        {
            this.SetAttributeAsEnum(metaProperty, enumValue);
            return;
        }

        ValidatePropertyValueAssign(metaProperty, 
            value, CimMetaPropertyKind.Attribute);

        if (CanChangeProperty(metaProperty, value) == false)
        {
            return;
        }

        if (_PropertiesData.ContainsKey(metaProperty) == false)
        {
            _PropertiesData.TryAdd(metaProperty, null);
        }

        if (_PropertiesData.TryGetValue(metaProperty, out var old))
        {
            if (value == null && old == null || old?.Equals(value) == true)
            {
                return;
            }

            if (value == null)
            {
                _PropertiesData.Remove(metaProperty, out var _);
            }
            else
            {
                _PropertiesData[metaProperty] = value;

                if (value is IModelObject compound)
                {
                    SubscribeToCompoundChanges(metaProperty, compound);
                }
            }

            OnPropertyChanged(new CimMetaAttributeChangedEventArgs(
                metaProperty, old, value));
        }
        else
        {
            throw new ArgumentException(
                $"Attribute {metaProperty.ShortName} does not exist!");
        }
    }

    public override void SetAttribute<T>(string attributeName, T? value)
        where T: default
    {
        var metaProperty = TryGetMetaPropertyByName(attributeName);
        if (metaProperty != null)
        {
            SetAttribute<T>(metaProperty, value);
            return;
        }

        throw new ArgumentException(
            $"No such meta property with name {attributeName}!");
    }

    public override IModelObject InitializeCompoundAttribute(
        ICimMetaProperty metaProperty, bool reset = true)
    {
        if (metaProperty.PropertyDatatype == null)
        {
            throw new NotSupportedException(
                $"Undefined compound property {metaProperty.ShortName} class type!");
        }

        if (GetAttribute(metaProperty) is IModelObject currentValue 
            && reset == false)
        {
            return currentValue;
        }

        if (InternalTypeLib != null)
        {
            var compoundObject = InternalTypeLib.CreateCompoundInstance(
                new ModelObjectFactory(),
                metaProperty.PropertyDatatype
            );

            if (compoundObject != null)
            {
                SetAttribute(metaProperty, compoundObject);
                return compoundObject;
            }
        }

        throw new NotSupportedException(
            $"Typelib error while {metaProperty.ShortName} compound object creation!");
    }

    public override IModelObject InitializeCompoundAttribute(
        string attributeName, bool reset = true)
    {
        var metaProperty = TryGetMetaPropertyByName(attributeName);
        if (metaProperty != null)
        {
            return InitializeCompoundAttribute(metaProperty, reset);
        }

        throw new ArgumentException(
            $"No such meta property with name {attributeName}!");
    }

    #endregion AttributesLogic

    #region Assocs11Logic

    public override T? GetAssoc1To1<T>(ICimMetaProperty metaProperty)
        where T: default
    {
        if (GetDataByProperty(metaProperty, CimMetaPropertyKind.Assoc1To1)
            is T typedObject)
        {
            return typedObject;
        }

        return default;
    }

    public override T? GetAssoc1To1<T>(string assocName)
        where T: default
    {
        var metaProperty = TryGetMetaPropertyByName(assocName);
        if (metaProperty != null)
        {
            return GetAssoc1To1<T>(metaProperty);
        }

        throw new ArgumentException(
            $"No such meta property with name {assocName}!");
    }

     public override void SetAssoc1To1(ICimMetaProperty metaProperty, 
        IModelObject? obj)
     {
        ValidatePropertyValueAssign(metaProperty, 
            obj, CimMetaPropertyKind.Assoc1To1);

        if (CanChangeProperty(metaProperty, obj) == false)
        {
            return;
        }

        if (_PropertiesData.ContainsKey(metaProperty) == false)
        {
            if (obj != null)
            {
                _PropertiesData.TryAdd(metaProperty, null);
            }
            else
            {
                return;
            }
        }

        if (_PropertiesData.TryGetValue(metaProperty, out var value))
        {
            if (value is IModelObject referenceObject
                && referenceObject.OID.Equals(obj?.OID))
            {
                return;
            }
            
            if (_PropertiesData[metaProperty] == obj)
            {
                return;
            }

            var assocObj = value as IModelObject;

            SetAssociationWithInverse(metaProperty, assocObj, obj);

            if (_PropertiesData[metaProperty] == null)
            {
                _PropertiesData.Remove(metaProperty, out var _);
            }

            OnPropertyChanged(new CimMetaAssocChangedEventArgs(
                metaProperty, assocObj, obj));
        }
        else
        {
            throw new ArgumentException(
                $"Association {metaProperty.ShortName} does not exist!");
        }
     }

    public override void SetAssoc1To1(string assocName, IModelObject? obj)
    {
        var metaProperty = TryGetMetaPropertyByName(assocName);
        if (metaProperty != null)
        {
            SetAssoc1To1(metaProperty, obj);
            return;
        }

        throw new ArgumentException(
            $"No such meta property with name {assocName}!");
    }

    #endregion Assocs11Logic

    #region Assocs1MLogic

    public override IModelObject[] GetAssoc1ToM(ICimMetaProperty metaProperty)
    {
        var data = GetDataByProperty(metaProperty, 
            CimMetaPropertyKind.Assoc1ToM) as ICollection<IModelObject>;

        if (data != null)
        {
            return data.ToArray();
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

        throw new ArgumentException(
            $"No such meta property with name {assocName}!");
    }

    public override T[] GetAssoc1ToM<T>(ICimMetaProperty metaProperty)
    {
        return GetAssoc1ToM(metaProperty).OfType<T>().ToArray();
    }

    public override T[] GetAssoc1ToM<T>(string assocName)
    {
        return GetAssoc1ToM(assocName).OfType<T>().ToArray();
    }

    public override void AddAssoc1ToM(ICimMetaProperty metaProperty, 
        IModelObject obj)
    {
        ValidatePropertyValueAssign(metaProperty, 
            obj, CimMetaPropertyKind.Assoc1ToM);

        if (CanChangeProperty(metaProperty, obj, false) == false)
        {
            return;
        }

        if (_PropertiesData.ContainsKey(metaProperty) == false)
        {
            _PropertiesData.TryAdd(metaProperty, new HashSet<IModelObject>(
                new ModelObjectOIDEqualityComparer()));
        }

        if (_PropertiesData.TryGetValue(metaProperty, out var value)
            && value is ICollection<IModelObject> assocCollection)
        {
            if (assocCollection.Contains(obj))
            {
                return;
            }

            SetAssociationWithInverse(metaProperty, null, obj);

            OnPropertyChanged(new CimMetaAssocChangedEventArgs(
                metaProperty, null, obj));
        }
        else
        {
            throw new ArgumentException(
                $"Association 1 to M {metaProperty.ShortName} does not exist!"); 
        }
    }

    public override void AddAssoc1ToM(string assocName, IModelObject obj)
    {
        var metaProperty = TryGetMetaPropertyByName(assocName);
        if (metaProperty != null)
        {
            AddAssoc1ToM(metaProperty, obj);
        }
        else
        {
            throw new ArgumentException(
                $"No such meta property with name {assocName}!");
        }
    }

    public override void RemoveAssoc1ToM(ICimMetaProperty metaProperty, 
        IModelObject obj)
    {       
        ValidatePropertyValueAssign(metaProperty, 
            obj, CimMetaPropertyKind.Assoc1ToM);
        
        if (CanChangeProperty(metaProperty, obj, true) == false)
        {
            return;
        }

        if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM
            && _PropertiesData.TryGetValue(metaProperty, out var value)
            && value is ICollection<IModelObject> assocCollection)
        {
            if (assocCollection.Contains(obj) == false)
            {
                return;
            }
            
            SetAssociationWithInverse(metaProperty, obj, null);
                            
            OnPropertyChanged(new CimMetaAssocChangedEventArgs(
                metaProperty, obj, null));
        }
    }

    public override void RemoveAssoc1ToM(string assocName, IModelObject obj)
    {
        var metaProperty = TryGetMetaPropertyByName(assocName);
        if (metaProperty != null)
        {
            RemoveAssoc1ToM(metaProperty, obj);
        }
        else
        {
            throw new ArgumentException(
                $"No such meta property with name {assocName}!");
        }
    }

    public override void RemoveAllAssocs1ToM(ICimMetaProperty metaProperty)
    {
        if (MetaClass.AllProperties.Contains(metaProperty)
            && _PropertiesData.ContainsKey(metaProperty) == false)
        {
            return;
        }

        if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM
            && _PropertiesData.TryGetValue(metaProperty, out var value)
            && value is ICollection<IModelObject> assocCollection)
        {
            foreach (var assocObject in assocCollection)   
            {
                RemoveAssoc1ToM(metaProperty, assocObject);
            }     
        }
        else
        {
            throw new ArgumentException(
                $"Association 1 to M {metaProperty.ShortName} does not exist!"); 
        }
    }

    public override void RemoveAllAssocs1ToM(string assocName)
    {
        var metaProperty = TryGetMetaPropertyByName(assocName);
        if (metaProperty != null)
        {
            RemoveAllAssocs1ToM(metaProperty);
        }
        else
        {
            throw new ArgumentException(
                $"No such meta property with name {assocName}!");
        }
    }

    #endregion Assocs1MLogic

    #region Statements

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
            _Statements.TryAdd(statementProperty, 
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
    #endregion Statements

    #region UtilsPrivate

    /// <summary>
    /// Validation of meta property assigning with value according schema.
    /// </summary>
    /// <param name="metaProperty">Meta property.</param>
    /// <param name="value">Value to assigning.</param>
    /// <param name="callerPropertyKind">Property kind.</param>
    /// <exception cref="Exception"></exception>
    /// <exception cref="ArgumentException"></exception>
    private void ValidatePropertyValueAssign(ICimMetaProperty metaProperty, 
        object? value, CimMetaPropertyKind callerPropertyKind)
    {
        if (metaProperty.PropertyKind != callerPropertyKind)
        {
            throw new ArgumentException(
                $"Invalid {metaProperty.ShortName} property kind " +
                $"{callerPropertyKind}");
        }

        if (MetaClass.AllProperties.Contains(metaProperty) == false)
        {
            throw new InvalidDataException(
                $"Property {metaProperty.ShortName} is not in " + 
                $"{MetaClass.ShortName} class domain");
        }

        if (metaProperty.PropertyKind == CimMetaPropertyKind.Attribute
            && CanAssignAttributeValue(metaProperty, value) == false)
        {
            throw new InvalidDataException(
                $"Unable assign {value} to {metaProperty.ShortName} property of " + 
                $"{MetaClass.ShortName} class domain");
        }
        else if ((metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1
            || metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM)
            && value is IModelObject modelObject
            && CanAssignAssociationObject(metaProperty, modelObject) == false)
        {
            throw new InvalidDataException(
                $"Unable set association {metaProperty.ShortName} of " + 
                $"{MetaClass.ShortName} class domain with " + 
                $"{modelObject.MetaClass.ShortName} type");
        }
    }

    /// <summary>
    /// Check attribute data compliance to set.
    /// </summary>
    /// <param name="metaProperty">Meta property.</param>
    /// <param name="value">Value to assigning.</param>
    /// <returns></returns>
    private static bool CanAssignAttributeValue(ICimMetaProperty metaProperty, 
        object? value)
    {
        if (metaProperty.PropertyDatatype == null)
        {
            return false;
        }

        if (value == null)
        {
            return true;
        }

        Type primitiveType = typeof(string);
        if (metaProperty.PropertyDatatype is ICimMetaDatatype datatype)
        {
            primitiveType = datatype.PrimitiveType;
        }

        if (value is IModelObject modelObject
            && metaProperty.PropertyDatatype.IsCompound
            && metaProperty.PropertyDatatype == modelObject.MetaClass)
        {
            return true;
        }
        else if (value is EnumValueObject enumWrappedValue
            && metaProperty.PropertyDatatype.IsEnum)
        {
            if (metaProperty.PropertyDatatype.AllIndividuals
                .Contains(enumWrappedValue.MetaEnumValue))
            {
                return true;
            }
        }
        else if (value.GetType().IsAssignableTo(primitiveType))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check association data complience to set.
    /// </summary>
    /// <param name="metaProperty">Meta property.</param>
    /// <param name="modelObject">Association object to assigning.</param>
    /// <returns></returns>
    private static bool CanAssignAssociationObject(ICimMetaProperty metaProperty, 
        IModelObject? modelObject)
    {
        if (metaProperty.PropertyDatatype == null)
        {
            return false;
        }

        if (modelObject == null 
            || modelObject is ModelObjectUnresolvedReference)
        {
            return true;
        }

        if (metaProperty.PropertyDatatype.Equals(modelObject.MetaClass)
            || modelObject.MetaClass.Extensions
                .Any(a => a.Equals(metaProperty.PropertyDatatype)))
        {
            return true;
        }

        var allAncestors = modelObject.MetaClass.AllAncestors;
        if (allAncestors.Any(a => a.Equals(metaProperty.PropertyDatatype) 
            || a.Extensions.Any(a => a.Equals(metaProperty.PropertyDatatype))))
        {
            return true;
        }

        return false;
    }

    private object? GetDataByProperty(ICimMetaProperty metaProperty,
        CimMetaPropertyKind? expectableKind = null)
    {
        if (_PropertiesData.TryGetValue(metaProperty, out var value)
            && (metaProperty.PropertyKind == expectableKind
                || expectableKind == null))
        {
            return value;
        }

        if (MetaClass.AllProperties.Contains(metaProperty)
            && (metaProperty.PropertyKind == expectableKind
                || expectableKind == null))
        {
            return null;
        }

        throw new ArgumentException(
            $"Property {metaProperty.ShortName} does not exist!");        
    }
    
    private void SetAssociationWithInverse(ICimMetaProperty metaProperty, 
        IModelObject? oldObj, IModelObject? newObj)
    {
        if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1)
        {
            _PropertiesData[metaProperty] = newObj;
        }
        else if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM
            && _PropertiesData[metaProperty] 
                is ICollection<IModelObject> assocCollection)
        {
            if (newObj != null)
            {
                assocCollection.Add(newObj);
            }
            else if (oldObj != null)
            {
                assocCollection.Remove(oldObj);
            }
        }

        if (oldObj != null && metaProperty.InverseProperty != null 
            && oldObj is not ModelObjectUnresolvedReference)
        {
            if (metaProperty.InverseProperty.PropertyKind
                == CimMetaPropertyKind.Assoc1To1)
            {
                oldObj.SetAssoc1To1(metaProperty.InverseProperty, null);
            }
            else if(metaProperty.InverseProperty.PropertyKind
                == CimMetaPropertyKind.Assoc1ToM)
            {
                oldObj.RemoveAssoc1ToM(metaProperty.InverseProperty, this);
            }
        }

        if (newObj != null && metaProperty.InverseProperty != null
            && newObj is not ModelObjectUnresolvedReference)
        {
            if (metaProperty.InverseProperty.PropertyKind
                == CimMetaPropertyKind.Assoc1To1)
            {
                newObj.SetAssoc1To1(metaProperty.InverseProperty, this);
            }
            else if(metaProperty.InverseProperty.PropertyKind
                == CimMetaPropertyKind.Assoc1ToM)
            {
                newObj.AddAssoc1ToM(metaProperty.InverseProperty, this);
            }
        }
    }
    
    private void InitStatementsCollections()
    {
        foreach (var statementProperty in MetaClass.AllProperties
            .Where(p => p.PropertyKind == CimMetaPropertyKind.Statements))
        {
            _Statements.TryAdd(statementProperty, 
                new HashSet<IModelObject>());
        }
    }

    #endregion UtilsPrivate

    private readonly ConcurrentDictionary<ICimMetaProperty, object?> 
    _PropertiesData;

    private readonly 
    ConcurrentDictionary<ICimMetaProperty, ICollection<IModelObject>> 
    _Statements = [];
}

public class ModelObjectFactory : IModelObjectFactory
{
    public System.Type ProduceType => typeof(ModelObject);

    public IModelObject Create(IOIDDescriptor oid, ICimMetaClass metaClass)
    {
        return new ModelObject(oid, metaClass);
    }
}
