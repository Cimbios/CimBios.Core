
using CimBios.Core.CimModel.CimDatatypeLib.Headers552;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.Schema.AutoSchema;

namespace CimBios.Core.CimModel.CimDatatypeLib;

/// <summary>
/// 
/// </summary>
public abstract class DifferenceObjectBase : IDifferenceObject
{
    public string OID { get; }

    public IReadOnlyCollection<ICimMetaProperty> ModifiedProperties
        => _ModifiedProperties;

    public IReadOnlyModelObject? OriginalObject => _OriginalObject;
    public IReadOnlyModelObject ModifiedObject => _ModifiedObject;

    protected abstract WeakModelObject? _OriginalObject { get; }
    protected WeakModelObject _ModifiedObject { get; }

    protected DifferenceObjectBase(string oid)
    {
        OID = oid;

        var descriptionMetaClass = new CimAutoClass(
            new (Description.ClassUri),
            nameof(Description),
            string.Empty
        );

        _ModifiedObject = new WeakModelObject(oid, descriptionMetaClass, false);
    }

    protected DifferenceObjectBase (string oid, ICimMetaClass metaClass)
    {
        OID = oid;

        _ModifiedObject = new WeakModelObject(oid, metaClass, false);
    }

    protected DifferenceObjectBase(IDifferenceObject differenceObject)
        : this (differenceObject.OID)
    {
        foreach (var prop in differenceObject.ModifiedProperties)
        {
            if (differenceObject.TryGetPropertyValue(prop,
                out var oldObj, out var newObj) == false)
            {
                continue;
            }

            ChangePropertyValue(prop, oldObj, newObj);
        }
    }

    public virtual void ChangeAttribute(ICimMetaProperty metaProperty, 
        object? fromValue, object? toValue)
    {
        if (_ModifiedProperties.Contains(metaProperty) == false)
        {           
            _ModifiedProperties.Add(metaProperty);
        }

        if (_OriginalObject?.MetaClass.HasProperty(metaProperty) == false)
        {
             _OriginalObject.SetAttribute(metaProperty, fromValue);
        }

        _ModifiedObject.SetAttribute(metaProperty, toValue);

        var oldVal = _OriginalObject?.GetAttribute(metaProperty);
        var newVal = _ModifiedObject.GetAttribute(metaProperty);

        if (oldVal == newVal)
        {
            _ModifiedProperties.Remove(metaProperty);
        }
    }

    public virtual void ChangeAssoc1(ICimMetaProperty metaProperty, 
        IModelObject? fromObject, IModelObject? toObject)
    {
        if (_ModifiedProperties.Contains(metaProperty) == false)
        {           
            _ModifiedProperties.Add(metaProperty);
        }

        if (_OriginalObject?.MetaClass.HasProperty(metaProperty) == false)
        {
             _OriginalObject.SetAssoc1To1(metaProperty, fromObject);
        }

        _ModifiedObject.SetAssoc1To1(metaProperty, toObject);

        var oldVal = _OriginalObject?.GetAssoc1To1<IModelObject>(metaProperty);
        var newVal = _ModifiedObject.GetAssoc1To1<IModelObject>(metaProperty);

        if (oldVal == newVal)
        {
            _ModifiedProperties.Remove(metaProperty);
        }
    }

    public virtual void AddToAssocM(ICimMetaProperty metaProperty, 
        IModelObject modelObject)
    {
        if (_ModifiedProperties.Contains(metaProperty) == false)
        {           
            _ModifiedProperties.Add(metaProperty);
        }

        _ModifiedObject.AddAssoc1ToM(metaProperty, modelObject);

        var oldVal = _OriginalObject?.GetAssoc1ToM<IModelObject>(metaProperty) ?? [];
        var newVal = _ModifiedObject.GetAssoc1ToM<IModelObject>(metaProperty);

        if (oldVal.Intersect(newVal).Contains(modelObject))
        {
            _ModifiedProperties.Remove(metaProperty);
        }
    }

    public virtual void RemoveFromAssocM(ICimMetaProperty metaProperty, 
        IModelObject modelObject)
    {
        if (_ModifiedProperties.Contains(metaProperty) == false)
        {           
            _ModifiedProperties.Add(metaProperty);
        }

        _OriginalObject?.AddAssoc1ToM(metaProperty, modelObject);

        var oldVal = _OriginalObject?.GetAssoc1ToM<IModelObject>(metaProperty) ?? [];
        var newVal = _ModifiedObject.GetAssoc1ToM<IModelObject>(metaProperty);

        if (!oldVal.Except(newVal).Any())
        {
            _ModifiedProperties.Remove(metaProperty);
        }
    }

    public virtual bool ChangePropertyValue(ICimMetaProperty metaProperty, 
            object? fromValue, object? toValue)
    {
        if (metaProperty.PropertyKind == CimMetaPropertyKind.Attribute)
        {
            ChangeAttribute(metaProperty, fromValue, toValue);
            
            return true;
        }
        else if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1)
        {
            ChangeAssoc1(metaProperty, fromValue as IModelObject, 
                toValue as IModelObject);

            return true;
        }
        else if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM)
        {
            if (fromValue == null && toValue is IModelObject toMObj)
            {
                AddToAssocM(metaProperty, toMObj);
            }

            if (toValue == null && fromValue is IModelObject fromMObj)
            {
                RemoveFromAssocM(metaProperty, fromMObj);
            }       

            if (fromValue == null && toValue 
                is ICollection<IModelObject> toMObjs)
            {
                foreach (var o in toMObjs)
                {
                    AddToAssocM(metaProperty, o);
                }
            }

            if (toValue == null && fromValue 
                is ICollection<IModelObject> fromMObjs)
            {
                foreach (var o in fromMObjs)
                {
                    RemoveFromAssocM(metaProperty, o);
                }
            }       

             return true;
        }   

         return false;
    }

    public bool TryGetPropertyValue (ICimMetaProperty metaProperty,
        out object? fromValue, out object? toValue)
    {
        fromValue = null;
        toValue = null;

        if (ModifiedProperties.Contains(metaProperty) == false)
        {
            return false;
        }

        if (metaProperty.PropertyKind == CimMetaPropertyKind.Attribute)
        {
            fromValue = _OriginalObject?.GetAttribute(metaProperty);
            toValue = _ModifiedObject.GetAttribute(metaProperty); 
        }
        else if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1)
        {
            fromValue = _OriginalObject?.GetAssoc1To1<IModelObject>(metaProperty);
            toValue = _ModifiedObject.GetAssoc1To1<IModelObject>(metaProperty);
        }
        else if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM)
        {
            fromValue = _OriginalObject?.GetAssoc1ToM<IModelObject>(metaProperty);
            toValue = _ModifiedObject.GetAssoc1ToM<IModelObject>(metaProperty);
        }

        return true;
    }

    protected HashSet<ICimMetaProperty> _ModifiedProperties = [];
}

/// <summary>
/// 
/// </summary>
/// <param name="oid"></param>
public class AdditionDifferenceObject (string oid, ICimMetaClass metaClass)
    : DifferenceObjectBase (oid, metaClass)
{
    protected override WeakModelObject? _OriginalObject => null;
}

/// <summary>
/// 
/// </summary>
/// <param name="oid"></param>
public class DeletionDifferenceObject (string oid, ICimMetaClass metaClass)
    : DifferenceObjectBase (oid, metaClass)
{
    protected override WeakModelObject? _OriginalObject => null;
}

/// <summary>
/// 
/// </summary>
/// <param name="oid"></param>
public class UpdatingDifferenceObject
    : DifferenceObjectBase
{
    protected override WeakModelObject? _OriginalObject { get; }

    public UpdatingDifferenceObject (string oid)
        : base (oid)
    {
        _OriginalObject = new WeakModelObject(oid, 
            _ModifiedObject.MetaClass, true);
    }
}