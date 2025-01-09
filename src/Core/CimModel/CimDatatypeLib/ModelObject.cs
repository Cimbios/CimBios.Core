using System.ComponentModel;
using System.Dynamic;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.CimDatatypeLib;

/// <summary>
/// Facade for data operations incapsulation.
/// </summary>
public class ModelObject : DynamicObject, IModelObject
{
    public IModelObject? Owner { get; }
    public string Uuid => _uuid;
    public bool IsAuto => _isAuto;
    public ICimMetaClass MetaClass => _MetaClass;

    public ModelObject(string uuid, ICimMetaClass metaClass, 
        bool isAuto = false)
    {
        _uuid = uuid;
        _isAuto = isAuto;

        _MetaClass = metaClass;
        _PropertiesData = [];
    }

    public bool HasProperty(string propertyName)
    {
        var metaProperty = TryGetMetaPropertyByName(propertyName);
        if (metaProperty != null)
        {
            return MetaClass.AllProperties.Contains(metaProperty);
        }

        return false;
    }

    public dynamic? AsDynamic()
    {
        return this;
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        var metaProperty = TryGetMetaPropertyByName(binder.Name);
        if (metaProperty != null)
        {
            if (metaProperty.PropertyKind == CimMetaPropertyKind.Attribute)
            {
                result = GetAttribute(metaProperty);
                return true;
            }
            else if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1)
            {
                result = GetAssoc1To1(metaProperty);
                return true;        
            }
            else if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM)
            {
                result = GetAssoc1ToM(metaProperty);
                return true;        
            }         
        }

        return base.TryGetMember(binder, out result);
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        var metaProperty = TryGetMetaPropertyByName(binder.Name);
        if (metaProperty != null)
        {
            if (metaProperty.PropertyKind == CimMetaPropertyKind.Attribute)
            {
                SetAttribute(metaProperty, value);
                return true;
            }
            else if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1)
            {
                SetAssoc1To1(metaProperty, value as IModelObject);
                return true;        
            }      
        }

        return base.TrySetMember(binder, value);
    }

    #region AttributesLogic

    public object? GetAttribute(ICimMetaProperty metaProperty)
    {
        return GetDataByProperty(metaProperty, CimMetaPropertyKind.Attribute);
    }

    public object? GetAttribute(string attributeName)
    {
        var metaProperty = TryGetMetaPropertyByName(attributeName);
        if (metaProperty != null)
        {
            return GetAttribute(metaProperty);
        }

        throw new ArgumentException(
            $"No such meta property with name {attributeName}!");
    }

    public T? GetAttribute<T>(ICimMetaProperty metaProperty) where T : class
    {
        return GetAttribute(metaProperty) as T;
    }

    public T? GetAttribute<T>(string attributeName) where T : class
    {
        var metaProperty = TryGetMetaPropertyByName(attributeName);
        if (metaProperty != null)
        {
            return GetAttribute<T>(metaProperty);
        }

        throw new ArgumentException(
            $"No such meta property with name {attributeName}!");
    }

    public void SetAttribute<T>(ICimMetaProperty metaProperty, T? value) 
        where T : class
    {
        if (CanChangeProperty(metaProperty) == false)
        {
            return;
        }

        if (_PropertiesData.ContainsKey(metaProperty) == false)
        {
            _PropertiesData.Add(metaProperty, null);
        }

        if (metaProperty.PropertyKind == CimMetaPropertyKind.Attribute
            && _PropertiesData.ContainsKey(metaProperty))
        {
            if (value == _PropertiesData[metaProperty])
            {
                return;
            }

            System.Type primitiveType = typeof(string);
            if (metaProperty.PropertyDatatype is ICimMetaDatatype datatype)
            {
                primitiveType = datatype.PrimitiveType;
            }

            if (value == null)
            {
                _PropertiesData.Remove(metaProperty);

                PropertyChanged?.Invoke(this, 
                    new CimMetaPropertyChangedEventArgs(metaProperty));
            }
            else if (value is IModelObject
                || value is Uri
                || value.GetType().IsAssignableTo(primitiveType))
            {
                _PropertiesData[metaProperty] = value;

                PropertyChanged?.Invoke(this, 
                    new CimMetaPropertyChangedEventArgs(metaProperty));
            }
            else
            {
                throw new ArgumentException(
                    $"Attribute {metaProperty.ShortName} can not be assigned by {value}!");
            }
        }
        else
        {
            throw new ArgumentException(
                $"Attribute {metaProperty.ShortName} does not exist!");
        }
    }

    public void SetAttribute<T>(string attributeName, T? value) where T : class
    {
        var metaProperty = TryGetMetaPropertyByName(attributeName);
        if (metaProperty != null)
        {
            SetAttribute<T>(metaProperty, value);
        }

        throw new ArgumentException(
            $"No such meta property with name {attributeName}!");
    }

    #endregion AttributesLogic

    #region Assocs11Logic

    public IModelObject? GetAssoc1To1(ICimMetaProperty metaProperty)
    {
        return GetDataByProperty(metaProperty, CimMetaPropertyKind.Assoc1To1)
            as IModelObject;
    }

    public IModelObject? GetAssoc1To1(string assocName)
    {
        var metaProperty = TryGetMetaPropertyByName(assocName);
        if (metaProperty != null)
        {
            return GetAssoc1To1(metaProperty);
        }

        throw new ArgumentException(
            $"No such meta property with name {assocName}!");
    }

     public void SetAssoc1To1(ICimMetaProperty metaProperty, IModelObject? obj)
     {
        if (CanChangeProperty(metaProperty) == false)
        {
            return;
        }

        if (obj != null && _PropertiesData.ContainsKey(metaProperty) == false)
        {
            _PropertiesData.Add(metaProperty, null);
        }

        if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1
            && _PropertiesData.ContainsKey(metaProperty))
        {
            if (_PropertiesData[metaProperty] == obj)
            {
                return;
            }

            var assocObj = _PropertiesData[metaProperty] as IModelObject;

            SetAssociationWithInverse(metaProperty, assocObj, obj);

            if (_PropertiesData[metaProperty] == null)
            {
                _PropertiesData.Remove(metaProperty);
            }

            PropertyChanged?.Invoke(this, 
                new CimMetaPropertyChangedEventArgs(metaProperty));
        }
        else
        {
            throw new ArgumentException(
                $"Association {metaProperty.ShortName} does not exist!");
        }
     }

    public void SetAssoc1To1(string assocName, IModelObject? obj)
    {
        var metaProperty = TryGetMetaPropertyByName(assocName);
        if (metaProperty != null)
        {
            SetAssoc1To1(metaProperty, obj);
        }

        throw new ArgumentException(
            $"No such meta property with name {assocName}!");
    }

    #endregion Assocs11Logic

    #region Assocs1MLogic

    public IModelObject[] GetAssoc1ToM(ICimMetaProperty metaProperty)
    {
        var data = GetDataByProperty(metaProperty, 
            CimMetaPropertyKind.Assoc1ToM) as ICollection<IModelObject>;

        if (data != null)
        {
            return data.ToArray();
        }

        return [];
    }

    public IModelObject[] GetAssoc1ToM(string assocName)
    {
        var metaProperty = TryGetMetaPropertyByName(assocName);
        if (metaProperty != null)
        {
            return GetAssoc1ToM(metaProperty);
        }

        throw new ArgumentException(
            $"No such meta property with name {assocName}!");
    }

    public void AddAssoc1ToM(ICimMetaProperty metaProperty, IModelObject obj)
    {
        if (_PropertiesData.ContainsKey(metaProperty) == false)
        {
            _PropertiesData.Add(metaProperty, new HashSet<IModelObject>());
        }

        if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM
            && _PropertiesData.TryGetValue(metaProperty, out var value)
            && value is ICollection<IModelObject> assocCollection)
        {
            if (assocCollection.Contains(obj))
            {
                return;
            }

            SetAssociationWithInverse(metaProperty, null, obj);

            PropertyChanged?.Invoke(this, 
                new CimMetaPropertyChangedEventArgs(metaProperty));
        }
        else
        {
            throw new ArgumentException(
                $"Association 1 to M {metaProperty.ShortName} does not exist!"); 
        }
    }

    public void AddAssoc1ToM(string assocName, IModelObject obj)
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

    public void RemoveAssoc1ToM(ICimMetaProperty metaProperty, IModelObject obj)
    {
        if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM
            && _PropertiesData.TryGetValue(metaProperty, out var value)
            && value is ICollection<IModelObject> assocCollection)
        {
            if (assocCollection.Contains(obj) == false)
            {
                return;
            }
            
            SetAssociationWithInverse(metaProperty, obj, null);
                            
            PropertyChanged?.Invoke(this, 
                new CimMetaPropertyChangedEventArgs(metaProperty));
        }
        else
        {
            throw new ArgumentException(
                $"Association 1 to M {metaProperty.ShortName} does not exist!"); 
        }
    }

    public void RemoveAssoc1ToM(string assocName, IModelObject obj)
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

    public void RemoveAllAssocs1ToM(ICimMetaProperty metaProperty)
    {
        if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM
            && _PropertiesData.TryGetValue(metaProperty, out var value)
            && value is ICollection<IModelObject> assocCollection)
        {
            foreach (var assocObject in assocCollection)   
            {
                SetAssociationWithInverse(metaProperty, assocObject, null);
            }     

            PropertyChanged?.Invoke(this, 
                new CimMetaPropertyChangedEventArgs(metaProperty));   
        }
        else
        {
            throw new ArgumentException(
                $"Association 1 to M {metaProperty.ShortName} does not exist!"); 
        }
    }

    public void RemoveAllAssocs1ToM(string assocName)
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

    #region UtilsPrivate

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
            && newObj != null
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

    private bool CanChangeProperty(ICimMetaProperty metaProperty)
    {
        if (PropertyChanging != null)
        {
            var arg = new CanCancelPropertyChangingEventArgs(metaProperty, false);

            PropertyChanging.Invoke(this, arg);
            
            if (arg.Cancel == true)
            {
                return false;
            }
        }      

        return true;
    }

    private ICimMetaProperty? TryGetMetaPropertyByName(string name)
    {
        var splitted = name.Split('.');
        var isClassPropForm = splitted.Length.Equals(2);

        foreach (var property in MetaClass.AllProperties)
        {
            var propCPForm = $"{property.OwnerClass?.ShortName}.{property.ShortName}";

            if ((isClassPropForm && propCPForm == name)
                || (property.ShortName == name))
            {
                return property;
            }
        }

        return null;
    }

    #endregion UtilsPrivate

    private string _uuid = string.Empty;
    private bool _isAuto;

    private ICimMetaClass _MetaClass;
    private Dictionary<ICimMetaProperty, object?> _PropertiesData;

    public event PropertyChangedEventHandler? PropertyChanged;

    public delegate void CanCancelPropertyChangingEventHandler(object? sender, 
        CanCancelPropertyChangingEventArgs e);
    public event CanCancelPropertyChangingEventHandler? PropertyChanging;
}

/// <summary>
/// Class for unfindable by reference object in model
/// ObjectData - ClassType means predicate URI
/// </summary>
public sealed class ModelObjectUnresolvedReference 
    : ModelObject, IModelObject
{
    public Uri Predicate => MetaClass.BaseUri;

    public ModelObjectUnresolvedReference(string uuid, ICimMetaClass metaClass)
        : base(uuid, metaClass, true)
    {
    }
}

/// <summary>
/// Class for class instance from schema.
/// </summary>
public sealed class CimSchemaIndividualModelObject 
    : ModelObject, IModelObject
{
    public Uri ClassType => MetaClass.BaseUri;

    public CimSchemaIndividualModelObject(string uuid, ICimMetaClass metaClass)
        : base(uuid, metaClass, false)
    {
    }
}
