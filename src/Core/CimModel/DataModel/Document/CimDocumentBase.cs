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

using System.Collections.Immutable;
using System.ComponentModel;
using System.Text;
using CimBios.Core.CimModel.DataModel.Utils;
using CimBios.Core.CimModel.DatatypeLib;
using CimBios.Core.CimModel.DatatypeLib.ModelObject;
using CimBios.Core.CimModel.DatatypeLib.OID;
using CimBios.Core.CimModel.DatatypeLib.TypeLib;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.RdfSerializer.DeserializationResult;
using CimBios.Core.CimModel.Schema;
using Serilog;

namespace CimBios.Core.CimModel.DataModel.Document;

public abstract class CimDocumentBase : ICimDataModel
{
    protected CimDocumentBase(ICimSchema cimSchema, ICimDatatypeLib typeLib,
        IOIDDescriptorFactory oidDescriptorFactory, ILogger? logger = null)
    {
        Objects = [];

        Schema = cimSchema;
        TypeLib = typeLib;
        OIDDescriptorFactory = oidDescriptorFactory;

        Logger = logger;
    }
    
    protected ILogger? Logger { get; }

    protected IReadOnlyCollection<ModelObjectUnresolvedReference>
        UnresolvedReferences { get; private set; } = [];

    /// <summary>
    ///     All cached objects collection (uuid to IModelObject).
    /// </summary>
    protected Dictionary<IOIDDescriptor, IModelObject> Objects { get; set; }

    public virtual Model? ModelDescription { get; protected set; }

    public virtual ICimSchema Schema { get; }

    public virtual ICimDatatypeLib TypeLib { get; }

    public virtual IOIDDescriptorFactory OIDDescriptorFactory { get; }
        = new UuidDescriptorFactory();

    public abstract IEnumerable<IModelObject> GetAllObjects();
    public abstract IEnumerable<T> GetObjects<T>() where T : IModelObject;
    public abstract IEnumerable<IModelObject> GetObjects(ICimMetaClass metaClass);
    public abstract IModelObject? GetObject(IOIDDescriptor oid);
    public abstract T? GetObject<T>(IOIDDescriptor oid) where T : IModelObject;
    
    public virtual IModelObject? GetObject(string oid)
        => GetObject(OIDDescriptorFactory.Create(oid));
    public virtual T? GetObject<T>(string oid) where T : IModelObject
        => GetObject<T>( OIDDescriptorFactory.Create(oid));
    
    public abstract bool RemoveObject(IOIDDescriptor oid);
    public abstract bool RemoveObject(IModelObject modelObject);
    public abstract void RemoveObjects(IEnumerable<IModelObject> modelObjects);
    public abstract IModelObject CreateObject(IOIDDescriptor oid, ICimMetaClass metaClass);
    public abstract T CreateObject<T>(IOIDDescriptor oid) where T : class, IModelObject;

    public event CimDataModelObjectPropertyChangedEventHandler?
        ModelObjectPropertyChanged;

    public event CimDataModelObjectStorageChangedEventHandler?
        ModelObjectStorageChanged;

    /// <summary>
    ///     Load CIM model to context via stream reader and custom schema.
    /// </summary>
    public virtual void Load(StreamReader streamReader,
        IRdfSerializerFactory serializerFactory,
        ICimSchema cimSchema)
    {
        Logger?.ForContext<CimDocumentBase>().Information("Loading model ...");

        IDeserializationResult? result = null;
        try
        {
            var serializer = serializerFactory.Create(cimSchema,
                TypeLib, OIDDescriptorFactory, Logger);
            
            serializer.BaseUri = new Uri(OIDDescriptorFactory.Namespace);
            
            Objects = [];
            result = serializer.Deserialize(streamReader);

            foreach (var ns in result.Namespaces)
                cimSchema.Namespaces.TryAdd(ns.Key, ns.Value);
            
            PushDeserializedObjects(result.ModelObjects);
        }
        catch (Exception ex)
        {
            Logger?.ForContext<CimDocumentBase>()
                .Fatal(ex, "Deserialization failed");

            throw;
        }
        finally
        {
            streamReader.Close();

            if (result != null) UnresolvedReferences = result.UnresolvedReferences;
        }
    }

    /// <summary>
    ///     Load CIM model to context via stream reader.
    /// </summary>
    public void Load(StreamReader streamReader,
        IRdfSerializerFactory serializerFactory)
    {
        Load(streamReader, serializerFactory, Schema);
    }

    /// <summary>
    ///     Load CIM model to context by path.
    /// </summary>
    public void Load(string path,
        IRdfSerializerFactory serializerFactory,
        ICimSchema cimSchema)
    {
        Load(new StreamReader(path, Encoding.Default), 
            serializerFactory, cimSchema);
    }

    /// <summary>
    ///     Load CIM model to context by path.
    /// </summary>
    public void Load(string path, IRdfSerializerFactory serializerFactory)
    {
        Load(path, serializerFactory, Schema);
    }

    /// <summary>
    ///     Parse CIM model to context from string.
    /// </summary>
    public virtual void Parse(string content, IRdfSerializerFactory serializerFactory,
        ICimSchema cimSchema, Encoding? encoding = null)
    {
        encoding ??= Encoding.Default;
        var memoryStream = new MemoryStream(encoding.GetBytes(content));
        var stringReader = new StreamReader(memoryStream);
        Load(stringReader, serializerFactory, cimSchema);
    }

    /// <summary>
    ///     Parse CIM model to context from string.
    /// </summary>
    public void Parse(string content, IRdfSerializerFactory serializerFactory,
        Encoding? encoding = null)
    {
        Parse(content, serializerFactory, Schema, encoding);
    }

    /// <summary>
    ///     Write CIM model to stream writer.
    /// </summary>
    public virtual void Save(StreamWriter streamWriter,
        IRdfSerializerFactory serializerFactory,
        ICimSchema cimSchema)
    {
        var allObjects = new List<IModelObject>(Objects.Values.Count + 1);
        if (ModelDescription != null) allObjects.Add(ModelDescription);
        allObjects.AddRange(Objects.Values);
        var forSerializeObjects = allObjects.ToImmutableList();
        
        var serializer = serializerFactory.Create(cimSchema,
            TypeLib, OIDDescriptorFactory, Logger);

        try
        {
            serializer.BaseUri = new Uri(OIDDescriptorFactory.Namespace);
            serializer.Serialize(streamWriter, forSerializeObjects);
        }
        catch (Exception ex)
        {
            Logger?.ForContext<CimDocumentBase>()
                .Fatal(ex, "Serialization failed");
            throw;
        }
        finally
        {
            streamWriter.Close();
        }
    }

    /// <summary>
    ///     Write CIM model to stream writer.
    /// </summary>
    public virtual void Save(StreamWriter streamWriter,
        IRdfSerializerFactory serializerFactory)
    {
        Save(streamWriter, serializerFactory, Schema);
    }

    /// <summary>
    ///     Save CIM model to file.
    /// </summary>
    public virtual void Save(string path, IRdfSerializerFactory serializerFactory,
        ICimSchema cimSchema)
    {
        Save(new StreamWriter(path), serializerFactory, cimSchema);
    }

    /// <summary>
    ///     Save CIM model to file.
    /// </summary>
    public virtual void Save(string path, IRdfSerializerFactory serializerFactory)
    {
        Save(new StreamWriter(path), serializerFactory, Schema);
    }

    /// <summary>
    ///     Push deserialized model objects to storage.
    /// </summary>
    /// <param name="cache">Model objects collection to push.</param>
    protected abstract void PushDeserializedObjects(
        IEnumerable<IModelObject> cache);

    /// <summary>
    ///     Event fires on any model object property changed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void OnModelObjectPropertyChanged(object? sender,
        PropertyChangedEventArgs e)
    {
        if (sender is not IModelObject modelObject
            || e is not CimMetaPropertyChangedEventArgs cimEv)
            return;

        ModelObjectPropertyChanged?.Invoke(this, modelObject, cimEv);
    }

    /// <summary>
    ///     Event fires on any model object property changing request.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    protected void OnModelObjectPropertyChanging(object? sender,
        CanCancelPropertyChangingEventArgs e)
    {
        if (e is CanCancelAssocChangingEventArgs assocChanging)
        {
            if (assocChanging.ModelObject != null
                && assocChanging.ModelObject is not ModelObjectUnresolvedReference)
            {
                var contextObject = GetObject(assocChanging.ModelObject.OID);
                if (assocChanging.ModelObject.Equals(contextObject)) return;
                
                e.Cancel = true;
                throw new InvalidDataException(
                    "This context does not contains sending association object!");
            }
        }
    }

    /// <summary>
    ///     Event fires on object add or removed from document storage.
    /// </summary>
    /// <param name="modelObject"></param>
    /// <param name="changeType"></param>
    protected void OnModelObjectStorageChanged(IModelObject modelObject,
        CimDataModelObjectStorageChangeType changeType)
    {
        if (modelObject.OID is AutoDescriptor) return;

        ModelObjectStorageChanged?.Invoke(this, modelObject,
            new CimDataModelObjectStorageChangedEventArgs(changeType));
    }
}
