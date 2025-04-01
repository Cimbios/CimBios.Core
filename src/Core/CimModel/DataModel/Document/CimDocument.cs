using System.Data;
using CimBios.Core.CimModel.CimDataModel.Utils;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.Headers552;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.CimDataModel;

/// <summary>
/// Instance of CIM model in Rdf/* format.
/// Supports input and output operations for CIM objects.
/// </summary>
public class CimDocument(ICimSchema cimSchema, ICimDatatypeLib typeLib, 
    IOIDDescriptorFactory oidDescriptorFactory) 
    : CimDocumentBase(cimSchema, typeLib, oidDescriptorFactory), ICimDataModel
{
    public override IEnumerable<IModelObject> GetAllObjects()
    {
        return _Objects.Values;
    }

    public override IEnumerable<T> GetObjects<T>() where T : default
    {
        return _Objects.Values.OfType<T>();
    }

    public override IEnumerable<IModelObject> GetObjects(ICimMetaClass metaClass)
    {
        return _Objects.Values.Where(o => o.MetaClass == metaClass);
    }

    public override IModelObject? GetObject(IOIDDescriptor oid)
    {
        if (_Objects.TryGetValue(oid, out var instance)
            && instance is not AutoDescriptor
            && !instance.MetaClass.IsCompound)
        {
            return instance;
        }
        else
        {
            return null;
        }
    }

    public override T? GetObject<T>(IOIDDescriptor oid) where T : default
    {
        IModelObject? modelObject = GetObject(oid);
        if (modelObject != null && modelObject is T typedObject)
        {
            return typedObject;
        }

        return default;
    }

    public override bool RemoveObject(IOIDDescriptor oid)
    {
        if (_Objects.TryGetValue(oid, out var removingObject)
            && _Objects.Remove(oid) == true)
        { 
            UnlinkAllModelObjectAssocs(removingObject);

            removingObject.PropertyChanged -= OnModelObjectPropertyChanged;

            OnModelObjectStorageChanged(removingObject, 
                CimDataModelObjectStorageChangeType.Remove);

            return true;
        }

        return false;
    }

    public override bool RemoveObject(IModelObject modelObject)
    {
        return RemoveObject(modelObject.OID);
    }

    public override void RemoveObjects(IEnumerable<IModelObject> modelObjects)
    {
        foreach (var modelObject in modelObjects)
        {
            RemoveObject(modelObject);
        }
    }

    public override IModelObject CreateObject(IOIDDescriptor oid, 
        ICimMetaClass metaClass)
    {
        if (oid.IsEmpty)
        {
            throw new ArgumentException("OID cannot be empty!");
        }   

        var instance = TypeLib.CreateInstance(
            new ModelObjectFactory(), oid, metaClass);

        if (instance == null)
        {
            throw new NotSupportedException("TypeLib instance creation failed!");
        }

        AddObjectToStorage(instance);

        return instance;
    }

    public override T CreateObject<T>(IOIDDescriptor oid) where T : class
    {
        if (oid.IsEmpty)
        {
            throw new ArgumentException("OID cannot be empty!");
        }   

        var instance = TypeLib.CreateInstance<T>(oid);

        if (instance == null)
        {
            throw new NotSupportedException("TypeLib instance creation failed!");
        }

        AddObjectToStorage(instance);

        return instance;
    }

    private void AddObjectToStorage(IModelObject modelObject)
    {
        if (_Objects.ContainsKey(modelObject.OID))
        {
            throw new ArgumentException(
                $"Object with OID:{modelObject.OID} already exists!");
        }

        _Objects.Add(modelObject.OID, modelObject);
        modelObject.PropertyChanged += OnModelObjectPropertyChanged;

        OnModelObjectStorageChanged(modelObject, 
            CimDataModelObjectStorageChangeType.Add);

        ResolveReferencesWithObject(modelObject);
    }

    private void ResolveReferencesWithObject(IModelObject modelObject)
    {
        foreach (var refObj in _UnresolvedReferences
            .Where(o => o.OID == modelObject.OID))
        {
            refObj.ResolveWith(modelObject);
        }
    }

    protected override void PushDeserializedObjects(
        IEnumerable<IModelObject> cache)
    {
        _Objects = cache.AsParallel().ToDictionary(k => k.OID, v => v);

        foreach (var obj in _Objects.Values)
        {
            if (obj is FullModel fullModel)
            {
                ModelDescription = fullModel;
                _Objects.Remove(obj.OID);
                continue;
            }

            obj.PropertyChanged += OnModelObjectPropertyChanged;
            obj.PropertyChanging += OnModelObjectPropertyChanging;
        }
    }

    private static void UnlinkAllModelObjectAssocs(IModelObject modelObject)
    {
        foreach (var assoc in modelObject.MetaClass.AllProperties)
        {
            if (assoc.PropertyKind == CimMetaPropertyKind.Assoc1To1)
            {
                modelObject.SetAssoc1To1(assoc, null);
            }
            else if (assoc.PropertyKind == CimMetaPropertyKind.Assoc1ToM)
            {
                modelObject.RemoveAllAssocs1ToM(assoc);
            }
        }
    }
}
