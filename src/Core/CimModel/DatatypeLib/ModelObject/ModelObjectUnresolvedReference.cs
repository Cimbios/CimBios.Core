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

using System.ComponentModel;
using CimBios.Core.CimModel.DatatypeLib.OID;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.Schema.AutoSchema;

namespace CimBios.Core.CimModel.DatatypeLib.ModelObject;

/// <summary>
///     Class for unfindable by reference object in model
///     ObjectData - ClassType means predicate URI
/// </summary>
public sealed class ModelObjectUnresolvedReference : IModelObject
{
    public ModelObjectUnresolvedReference(IOIDDescriptor oid,
        ICimMetaProperty metaProperty)
    {
        OID = oid;
        MetaClass = metaProperty.PropertyDatatype ?? throw new InvalidDataException();

        if (metaProperty.PropertyKind != CimMetaPropertyKind.Assoc1To1
            && metaProperty.PropertyKind != CimMetaPropertyKind.Assoc1ToM)
            throw new InvalidDataException();

        MetaProperty = metaProperty;
    }

    public ICimMetaProperty MetaProperty { get; }

    public ISet<IModelObject> WaitingObjects { get; }
        = new HashSet<IModelObject>();

    public IOIDDescriptor OID { get; }

    public ICimMetaClass MetaClass { get; }

    public event CanCancelPropertyChangingEventHandler? PropertyChanging
    {
        add => throw new NotSupportedException();

        remove { }
    }

    public event PropertyChangedEventHandler? PropertyChanged
    {
        add => throw new NotSupportedException();

        remove { }
    }

    public void SetAttribute<T>(ICimMetaProperty metaProperty, T? value)
    {
        throw new NotImplementedException();
    }

    public void SetAttribute<T>(string attributeName, T? value)
    {
        throw new NotImplementedException();
    }

    public void SetAssoc1To1(ICimMetaProperty metaProperty, IModelObject? obj)
    {
        throw new NotImplementedException();
    }

    public void SetAssoc1To1(string assocName, IModelObject? obj)
    {
        throw new NotImplementedException();
    }

    public void AddAssoc1ToM(ICimMetaProperty metaProperty, IModelObject obj)
    {
        throw new NotImplementedException();
    }

    public void AddAssoc1ToM(string assocName, IModelObject obj)
    {
        throw new NotImplementedException();
    }

    public void RemoveAssoc1ToM(ICimMetaProperty metaProperty, IModelObject obj)
    {
        throw new NotImplementedException();
    }

    public void RemoveAssoc1ToM(string assocName, IModelObject obj)
    {
        throw new NotImplementedException();
    }

    public void RemoveAllAssocs1ToM(ICimMetaProperty metaProperty)
    {
        throw new NotImplementedException();
    }

    public void RemoveAllAssocs1ToM(string assocName)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyModelObject AsReadOnly()
    {
        throw new NotImplementedException();
    }

    public void Shrink()
    {
        throw new NotImplementedException();
    }

    public object? GetAttribute(ICimMetaProperty metaProperty)
    {
        throw new NotImplementedException();
    }

    public object? GetAttribute(string attributeName)
    {
        throw new NotImplementedException();
    }

    public T? GetAttribute<T>(ICimMetaProperty metaProperty)
    {
        throw new NotImplementedException();
    }

    public T? GetAttribute<T>(string attributeName)
    {
        throw new NotImplementedException();
    }

    public T? GetAssoc1To1<T>(ICimMetaProperty metaProperty) where T : IModelObject
    {
        throw new NotImplementedException();
    }

    public T? GetAssoc1To1<T>(string assocName) where T : IModelObject
    {
        throw new NotImplementedException();
    }

    public IModelObject[] GetAssoc1ToM(ICimMetaProperty metaProperty)
    {
        throw new NotImplementedException();
    }

    public IModelObject[] GetAssoc1ToM(string assocName)
    {
        throw new NotImplementedException();
    }

    public T[] GetAssoc1ToM<T>(ICimMetaProperty metaProperty) where T : IModelObject
    {
        throw new NotImplementedException();
    }

    public T[] GetAssoc1ToM<T>(string assocName) where T : IModelObject
    {
        throw new NotImplementedException();
    }

    public bool HasProperty(string propertyName)
    {
        throw new NotImplementedException();
    }

    public IModelObject? GetAssoc1To1(ICimMetaProperty metaProperty)
    {
        throw new NotImplementedException();
    }

    public IModelObject? GetAssoc1To1(string assocName)
    {
        throw new NotImplementedException();
    }

    public IModelObject InitializeCompoundAttribute(ICimMetaProperty metaProperty, bool reset = true)
    {
        throw new NotImplementedException();
    }

    public IModelObject InitializeCompoundAttribute(string attributeName, bool reset = true)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Resolve unresolved references in waiting objects.
    /// </summary>
    /// <param name="modelObject">Full model object.</param>
    /// <exception cref="InvalidDataException"></exception>
    public void ResolveWith(IModelObject modelObject)
    {
        if (modelObject is ModelObjectUnresolvedReference) 
            throw new InvalidOperationException(
                $"Unexpected null inverse association: {MetaProperty.BaseUri}");

        foreach (var waiting in WaitingObjects)
        {
            // Before resolve ref we should reset current unresolved refs in object
            CleanInverse(modelObject, waiting);

            if (MetaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1)
            {
                waiting.SetAssoc1To1(MetaProperty, null);
                waiting.SetAssoc1To1(MetaProperty, modelObject);
            }
            else if (MetaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM)
            {
                waiting.RemoveAssoc1ToM(MetaProperty, this);
                waiting.AddAssoc1ToM(MetaProperty, modelObject);
            }
        }

        WaitingObjects.Clear();
    }

    private void CleanInverse(IModelObject referenceObject,
        IModelObject waitingObject)
    {
        if (MetaProperty.InverseProperty == null) throw new InvalidDataException();

        if (MetaProperty.InverseProperty.PropertyKind
            == CimMetaPropertyKind.Assoc1To1)
        {
            var assocNow = referenceObject.GetAssoc1To1(
                MetaProperty.InverseProperty);

            if (assocNow?.OID.Equals(waitingObject.OID) ?? false)
                referenceObject.SetAssoc1To1(
                    MetaProperty.InverseProperty, null);
        }
        else if (MetaProperty.InverseProperty.PropertyKind
                 == CimMetaPropertyKind.Assoc1ToM)
        {
            referenceObject.RemoveAssoc1ToM(
                MetaProperty.InverseProperty, waitingObject);
        }
    }
}