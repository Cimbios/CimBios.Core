/*
*    CimBios.Core - Common Information Model (IEC61970) I/O Library
*    Copyright (C) 2025 Yuri A. Kovalenko a.k.a belizahrt <belizahrt@gmail.com>
*
*    This program is free software: you can redistribute it and/or modify
*    it under the terms of the GNU General Public License as published by
*    the Free Software Foundation, either version 3 of the License, or
*    (at your option) any later version.
*
*    This program is distributed in the hope that it will be useful,
*    but WITHOUT ANY WARRANTY; without even the implied warranty of
*    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*    GNU General Public License for more details.
*
*    You should have received a copy of the GNU General Public License
*    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using CimBios.Core.CimModel.DataModel.Utils;
using CimBios.Core.CimModel.DatatypeLib;
using CimBios.Core.CimModel.DatatypeLib.ModelObject;
using CimBios.Core.CimModel.DatatypeLib.OID;
using CimBios.Core.CimModel.DatatypeLib.TypeLib;
using CimBios.Core.CimModel.Schema;
using Serilog;

namespace CimBios.Core.CimModel.DataModel.Document;

/// <summary>
///     Instance of CIM model in Rdf/* format.
///     Supports input and output operations for CIM objects.
/// </summary>
public class CimDocument(
    ICimSchema cimSchema,
    ICimDatatypeLib typeLib,
    IOIDDescriptorFactory oidDescriptorFactory,
    ILogger? logger=null)
    : CimDocumentBase(cimSchema, typeLib, oidDescriptorFactory, logger), ICimDataModel
{
    public override IEnumerable<IModelObject> GetAllObjects()
    {
        return Objects.Values;
    }

    public override IEnumerable<T> GetObjects<T>() where T : default
    {
        return Objects.Values.OfType<T>();
    }

    public override IEnumerable<IModelObject> GetObjects(ICimMetaClass metaClass)
    {
        return Objects.Values.Where(o => o.MetaClass == metaClass);
    }

    public override IModelObject? GetObject(IOIDDescriptor oid)
    {
        if (Objects.TryGetValue(oid, out var instance)
            && instance.OID is not AutoDescriptor
            && !instance.MetaClass.IsCompound)
            return instance;

        return null;
    }

    public override T? GetObject<T>(IOIDDescriptor oid) where T : default
    {
        var modelObject = GetObject(oid);
        if (modelObject is T typedObject) return typedObject;

        return default;
    }

    public override bool RemoveObject(IOIDDescriptor oid)
    {
        if (Objects.TryGetValue(oid, out var removingObject))
        {
            UnlinkAllModelObjectAssocs(removingObject);

            Objects.Remove(oid);

            OnModelObjectStorageChanged(removingObject,
                CimDataModelObjectStorageChangeType.Remove);

            removingObject.PropertyChanged -= OnModelObjectPropertyChanged;

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
        foreach (var modelObject in modelObjects) RemoveObject(modelObject);
    }

    public override IModelObject CreateObject(IOIDDescriptor oid,
        ICimMetaClass metaClass)
    {
        if (oid.IsEmpty) throw new ArgumentException("OID cannot be empty!");

        var instance = TypeLib.CreateInstance(
            new ModelObjectFactory(), oid, metaClass);

        if (instance == null) throw new NotSupportedException("TypeLib instance creation failed!");

        AddObjectToStorage(instance);

        return instance;
    }

    public override T CreateObject<T>(IOIDDescriptor oid) where T : class
    {
        if (oid.IsEmpty) throw new ArgumentException("OID cannot be empty!");

        var instance = TypeLib.CreateInstance<T>(oid);

        if (instance == null) throw new NotSupportedException("TypeLib instance creation failed!");

        AddObjectToStorage(instance);

        return instance;
    }

    private void AddObjectToStorage(IModelObject modelObject)
    {
        if (Objects.ContainsKey(modelObject.OID))
            throw new ArgumentException(
                $"Object with OID:{modelObject.OID} already exists!");

        switch (modelObject)
        {
            case FullModel fullModel:
                ModelDescription = fullModel;
                return;
            case DifferenceModel:
                throw new InvalidDataException("DifferenceModel cannot be added explicitly!");
        }

        Objects.Add(modelObject.OID, modelObject);
        modelObject.PropertyChanged += OnModelObjectPropertyChanged;

        OnModelObjectStorageChanged(modelObject,
            CimDataModelObjectStorageChangeType.Add);

        ResolveReferencesWithObject(modelObject);
    }

    private void ResolveReferencesWithObject(IModelObject modelObject)
    {
        foreach (var refObj in UnresolvedReferences
                     .Where(o => o.OID.Equals(modelObject.OID)))
            refObj.ResolveWith(modelObject);
    }

    protected override void PushDeserializedObjects(
        IEnumerable<IModelObject> cache)
    {
        Objects = cache.AsParallel().ToDictionary(k => k.OID, v => v);

        var needFullModel = true;
        foreach (var obj in Objects.Values)
        {
            if (obj is FullModel fullModel)
            {
                ModelDescription = fullModel;
                Objects.Remove(obj.OID);
                needFullModel = false;
                continue;
            }

            obj.PropertyChanged += OnModelObjectPropertyChanged;
            obj.PropertyChanging += OnModelObjectPropertyChanging;
        }

        if (!needFullModel) return;

        try
        {
            ModelDescription = TypeLib.CreateInstance<FullModel>(
                OIDDescriptorFactory.Create());

            if (ModelDescription == null)
            {
                Logger?.ForContext<CimDocument>()
                    .Error("Failed to create FullModel: it's not registered");

                return;
            }

            ModelDescription.created = DateTime.Now;
        }
        catch (Exception ex)
        {
            Logger?.ForContext<CimDocument>()
                .Error(ex, "Failed to create FullModel");
        }
    }

    private static void UnlinkAllModelObjectAssocs(IModelObject modelObject)
    {
        foreach (var assoc in modelObject.MetaClass.AllProperties)
            if (assoc.PropertyKind == CimMetaPropertyKind.Assoc1To1)
                modelObject.SetAssoc1To1(assoc, null);
            else if (assoc.PropertyKind == CimMetaPropertyKind.Assoc1ToM) 
                modelObject.RemoveAllAssocs1ToM(assoc);
    }
}
