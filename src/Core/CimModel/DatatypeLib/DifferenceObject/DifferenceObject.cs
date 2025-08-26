using CimBios.Core.CimModel.CimDatatypeLib.Headers552;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.CimDatatypeLib.Utils;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.Schema.AutoSchema;

namespace CimBios.Core.CimModel.CimDatatypeLib;

/// <summary>
///     Base class for difference objects, provides comparision access to changes.
/// </summary>
public abstract class DifferenceObjectBase : IDifferenceObject
{
    protected HashSet<ICimMetaProperty> _ModifiedProperties = [];

    protected DifferenceObjectBase(IOIDDescriptor oid)
    {
        OID = oid;

        var descriptionMetaClass = new CimAutoClass(
            new Uri(Description.ClassUri),
            nameof(Description),
            string.Empty
        );

        _ModifiedObject = new WeakModelObject(oid, descriptionMetaClass);

        MetaClass = descriptionMetaClass;
    }

    protected DifferenceObjectBase(IOIDDescriptor oid, ICimMetaClass metaClass)
    {
        OID = oid;

        _ModifiedObject = new WeakModelObject(oid, metaClass);

        MetaClass = metaClass;
    }

    protected DifferenceObjectBase(IModelObject modifiedObject)
    {
        OID = modifiedObject.OID;

        _ModifiedObject = new WeakModelObject(modifiedObject);

        _ModifiedProperties = _ModifiedObject
            .GetNotNullProperties().ToHashSet();

        MetaClass = modifiedObject.MetaClass;
    }

    protected DifferenceObjectBase(IDifferenceObject differenceObject)
        : this(differenceObject.OID)
    {
        foreach (var prop in differenceObject.ModifiedProperties)
        {
            if (differenceObject.TryGetPropertyValue(prop,
                    out var oldObj, out var newObj) == false)
                continue;

            ChangePropertyValue(prop, oldObj, newObj);
        }
    }

    protected abstract WeakModelObject? _OriginalObject { get; }
    protected WeakModelObject _ModifiedObject { get; }
    public IOIDDescriptor OID { get; }

    public ICimMetaClass MetaClass { get; }

    public bool HasProperty(string propertyName)
    {
        return ModifiedProperties.FirstOrDefault(p => p.ShortName == propertyName) != null;
    }

    public IReadOnlyCollection<ICimMetaProperty> ModifiedProperties
        => _ModifiedProperties;

    public IReadOnlyModelObject? OriginalObject => _OriginalObject?.AsReadOnly();
    public IReadOnlyModelObject ModifiedObject => _ModifiedObject.AsReadOnly();

    public virtual void ChangeAttribute(ICimMetaProperty metaProperty,
        object? fromValue, object? toValue)
    {
        if (_ModifiedProperties.Contains(metaProperty) == false) _ModifiedProperties.Add(metaProperty);

        if (fromValue is WeakModelObject oldCompoundMock
            && toValue is WeakModelObject newCompoundMock)
        {
            ChangeCompoundAttribute(metaProperty,
                oldCompoundMock, newCompoundMock);

            return;
        }

        if (_OriginalObject?.MetaClass.HasProperty(metaProperty) == false)
            _OriginalObject.SetAttribute(metaProperty, fromValue);

        _ModifiedObject.SetAttribute(metaProperty, toValue);

        var oldVal = _OriginalObject?.GetAttribute(metaProperty);
        var newVal = _ModifiedObject.GetAttribute(metaProperty);

        if (newVal is null && oldVal is null 
            || newVal is not null && newVal.Equals(oldVal))
        {
            _ModifiedProperties.Remove(metaProperty);
            _OriginalObject?.SetAttribute<object>(metaProperty, null);
            _ModifiedObject?.SetAttribute<object>(metaProperty, null);
        }
    }

    public virtual void ChangeAssoc1(ICimMetaProperty metaProperty,
        IModelObject? fromObject, IModelObject? toObject)
    {
        if (_ModifiedProperties.Contains(metaProperty) == false) _ModifiedProperties.Add(metaProperty);

        if (_OriginalObject?.MetaClass.HasProperty(metaProperty) == false)
            _OriginalObject.SetAssoc1To1(metaProperty, fromObject);

        _ModifiedObject.SetAssoc1To1(metaProperty, toObject);

        var oldVal = _OriginalObject?.GetAssoc1To1<IModelObject>(metaProperty);
        var newVal = _ModifiedObject.GetAssoc1To1<IModelObject>(metaProperty);

        if ((oldVal != null && oldVal.OID.Equals(newVal?.OID))
            || (newVal != null && newVal.OID.Equals(oldVal?.OID))
            || (oldVal == null && newVal == null))
        {
            _ModifiedProperties.Remove(metaProperty);
            _OriginalObject?.SetAssoc1To1(metaProperty, null);
            _ModifiedObject?.SetAssoc1To1(metaProperty, null);
        }
    }

    public virtual void AddToAssocM(ICimMetaProperty metaProperty,
        IModelObject modelObject)
    {
        var comparer = new ModelObjectOIDEqualityComparer();
        
        _ModifiedProperties.Add(metaProperty);

        if (_OriginalObject?.GetAssoc1ToM(metaProperty)
                .Contains(modelObject, comparer) ?? false)
            _OriginalObject.RemoveAssoc1ToM(metaProperty, modelObject);
        else
            _ModifiedObject.AddAssoc1ToM(metaProperty, modelObject);

        var oldVal = _OriginalObject?.GetAssoc1ToM<IModelObject>(metaProperty) ?? [];
        var newVal = _ModifiedObject.GetAssoc1ToM<IModelObject>(metaProperty);

        // if (oldVal.Intersect(newVal, comparer)
        //     .Contains(modelObject, comparer))
        if(oldVal.Length == 0 && newVal.Length == 0)
            _ModifiedProperties.Remove(metaProperty);
    }

    public virtual void RemoveFromAssocM(ICimMetaProperty metaProperty,
        IModelObject modelObject)
    {
        var comparer = new ModelObjectOIDEqualityComparer();
        
        _ModifiedProperties.Add(metaProperty);
        
        if (_ModifiedObject.GetAssoc1ToM(metaProperty)
                .Contains(modelObject, comparer))
            _ModifiedObject.RemoveAssoc1ToM(metaProperty, modelObject);
        else
            _OriginalObject?.AddAssoc1ToM(metaProperty, modelObject);

        var oldVal = _OriginalObject?.GetAssoc1ToM<IModelObject>(metaProperty) ?? [];
        var newVal = _ModifiedObject.GetAssoc1ToM<IModelObject>(metaProperty);

        // if (!oldVal.Except(newVal, new ModelObjectOIDEqualityComparer()).Any())
        //     _ModifiedProperties.Remove(metaProperty);
        if(oldVal.Length == 0 && newVal.Length == 0)
            _ModifiedProperties.Remove(metaProperty);
    }

    public virtual bool ChangePropertyValue(ICimMetaProperty metaProperty,
        object? fromValue, object? toValue)
    {
        if (metaProperty.PropertyKind == CimMetaPropertyKind.Attribute)
        {
            ChangeAttribute(metaProperty, fromValue, toValue);

            return true;
        }

        if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1)
        {
            ChangeAssoc1(metaProperty, fromValue as IModelObject,
                toValue as IModelObject);

            return true;
        }

        if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM)
        {
            if (fromValue == null && toValue is IModelObject toMObj) AddToAssocM(metaProperty, toMObj);

            if (toValue == null && fromValue is IModelObject fromMObj) RemoveFromAssocM(metaProperty, fromMObj);

            if (fromValue == null && toValue
                    is ICollection<IModelObject> toMObjs)
                foreach (var o in toMObjs)
                    AddToAssocM(metaProperty, o);

            if (toValue == null && fromValue
                    is ICollection<IModelObject> fromMObjs)
                foreach (var o in fromMObjs)
                    RemoveFromAssocM(metaProperty, o);

            return true;
        }

        return false;
    }

    public bool TryGetPropertyValue(ICimMetaProperty metaProperty,
        out object? fromValue, out object? toValue)
    {
        fromValue = null;
        toValue = null;

        if (ModifiedProperties.Contains(metaProperty) == false) return false;

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

    public void RemovePropertyChange(ICimMetaProperty metaProperty)
    {
        _ModifiedProperties.Remove(metaProperty);

        if (metaProperty.PropertyKind == CimMetaPropertyKind.Attribute)
        {
            _OriginalObject?.SetAttribute<object>(metaProperty, null);
            _ModifiedObject.SetAttribute<object>(metaProperty, null);
        }
        else if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1)
        {
            _OriginalObject?.SetAssoc1To1(metaProperty, null);
            _ModifiedObject.SetAssoc1To1(metaProperty, null);
        }
        else if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM)
        {
            _OriginalObject?.RemoveAllAssocs1ToM(metaProperty);
            _ModifiedObject.RemoveAllAssocs1ToM(metaProperty);
        }
    }

    private void ChangeCompoundAttribute(ICimMetaProperty metaProperty,
        WeakModelObject oldCompoundMock, WeakModelObject newCompoundMock)
    {
        var modifiedCompound = _ModifiedObject.GetAttribute(metaProperty)
            as IModelObject;
        modifiedCompound ??= _ModifiedObject
            .InitializeCompoundAttribute(metaProperty, false);
        modifiedCompound.CopyPropertiesFrom(newCompoundMock);
        
        var originalCompound = _OriginalObject?.GetAttribute(metaProperty)
            as IModelObject;
        if (originalCompound == null)
        {
            originalCompound = _OriginalObject?
                .InitializeCompoundAttribute(metaProperty, false);
            originalCompound?.CopyPropertiesFrom(oldCompoundMock);
        }
        
        if (originalCompound == null) return;
        
        var compoundDiff = ModelObjectsComparer.Compare(
            originalCompound, modifiedCompound, true);

        if (compoundDiff.ModifiedProperties.Count == 0) 
            _ModifiedProperties.Remove(metaProperty);
    }
}

/// <summary>
/// </summary>
public class AdditionDifferenceObject : DifferenceObjectBase
{
    public AdditionDifferenceObject(IOIDDescriptor oid, ICimMetaClass metaClass)
        : base(oid, metaClass)
    {
    }

    public AdditionDifferenceObject(IModelObject modifiedModelObject)
        : base(modifiedModelObject)
    {
    }
    
    protected override WeakModelObject? _OriginalObject => null;
}

/// <summary>
/// </summary>
public class DeletionDifferenceObject
    : DifferenceObjectBase
{
    public DeletionDifferenceObject(IOIDDescriptor oid, ICimMetaClass metaClass)
        : base(oid, metaClass)
    {
    }

    public DeletionDifferenceObject(IModelObject modifiedMOdelObject)
        : base(modifiedMOdelObject)
    {
    }

    protected override WeakModelObject? _OriginalObject => null;
}

/// <summary>
/// </summary>
public class UpdatingDifferenceObject
    : DifferenceObjectBase
{
    public UpdatingDifferenceObject(IOIDDescriptor oid)
        : base(oid)
    {
        _OriginalObject = new WeakModelObject(oid,
            _ModifiedObject.MetaClass);
    }

    public UpdatingDifferenceObject(IModelObject originalObject,
        IModelObject modifiedObject)
        : base(modifiedObject)
    {
        if (originalObject.MetaClass != modifiedObject.MetaClass)
            throw new NotSupportedException(
                "Origin and modified objects meta classes should be equals!");

        _OriginalObject = new WeakModelObject(originalObject);

        _ModifiedProperties =
            _OriginalObject.GetNotNullProperties()
                .Union(_ModifiedObject.GetNotNullProperties())
                .ToHashSet();
    }

    protected override WeakModelObject? _OriginalObject { get; }
}

/// <summary>
///     Extension methods for WeakModelObject class.
/// </summary>
internal static class WeakModelObjectExtensions
{
    internal static ICimMetaProperty[] GetNotNullProperties(
        this WeakModelObject modelObject)
    {
        var notNullProps = new List<ICimMetaProperty>();

        foreach (var metaProperty in modelObject.MetaClass.AllProperties)
            if (metaProperty.PropertyKind == CimMetaPropertyKind.Attribute
                && modelObject.GetAttribute(metaProperty) != null)
                notNullProps.Add(metaProperty);
            else if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1
                     && modelObject.GetAssoc1To1<IModelObject>(metaProperty) != null)
                notNullProps.Add(metaProperty);
            else if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM
                     && modelObject.GetAssoc1ToM(metaProperty).Length != 0)
                notNullProps.Add(metaProperty);

        return notNullProps.ToArray();
    }
}