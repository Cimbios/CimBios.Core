using System.Globalization;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.RdfIOLib;
using CimBios.Utils.ClassTraits;

namespace CimBios.Core.CimModel.RdfSerializer;

/// <summary>
/// Base serializer class provides (de)serialization functions.
/// </summary>
public abstract class RdfSerializerBase : ICanLog
{
    protected const string IdentifierPrefix = "#_";

    public ILogView Log => _Log;

    /// <summary>
    /// Cim schema rules.
    /// </summary>
    public ICimSchema Schema
    { get => _schema; set => _schema = value; }

    /// <summary>
    /// CIM data types library for contrete typed instances creating.
    /// </summary>
    public ICimDatatypeLib TypeLib
    { get => _typeLib; set => _typeLib = value; }

    public Uri BaseUri { get; set; } = 
        new Uri("http://cim.bios/serialization/model/rdfxml");

    /// <summary>
    /// Rdf reader abstract entity.
    /// </summary>
    protected abstract RdfReaderBase _RdfReader { get; }

    /// <summary>
    /// Rdf writer abstract entity.
    /// </summary>
    protected abstract RdfWriterBase _RdfWriter { get; }

    protected RdfSerializerBase(ICimSchema schema, ICimDatatypeLib datatypeLib) 
    {
        _schema = schema;
        _typeLib = datatypeLib;
        _Log = new PlainLogView(this);
    }

    /// <summary>
    /// Deserialize data provider data to IModelObject instances.
    /// <param name="settings">Serializer settings.</param>
    /// <returns>Deserializer IModelObject collection.</returns>
    /// </summary>
    public IEnumerable<IModelObject> Deserialize(StreamReader streamReader)
    {
        InitializeRdfReader(streamReader);
        var deserializedObjects = ReadObjects();
        ResetCache();

        return deserializedObjects;
    }

    /// <summary>
    /// Serialize IModelObject instances to data provider source.
    /// <param name="modelObjects">IModelObject collection for serialization.</param>
    /// <param name="settings">Serializer settings.</param>
    /// </summary>
    public void Serialize(StreamWriter streamWriter, 
        IEnumerable<IModelObject> modelObjects)
    {
        InitializeRdfWriter(streamWriter);
        WriteObjects(modelObjects);
        ResetCache();
    }

    #region SerializerBlock

    private void InitializeRdfWriter(StreamWriter streamWriter)
    {
        if (streamWriter == null)
        {
            throw new Exception("No data stream for write!");
        }

        _streamWriter = streamWriter;

        InitializeWriterNamespaces();

        _RdfWriter.Open(streamWriter);
    }

    private void WriteObjects(IEnumerable<IModelObject> modelObjects)
    {
        if (_streamWriter == null)
        {
            throw new Exception("Serializer writer has not been intialized!");
        }

        var objsToWrite = new List<RdfNode>();
        foreach (var modelObject in modelObjects)
        {
            var moNode = ModelObjectToRdfNode(modelObject);
            if (moNode == null)
            {
                continue;
            }

            objsToWrite.Add(moNode);
        }

        _RdfWriter.WriteAll(objsToWrite);
        _RdfWriter.Close();
    }

    /// <summary>
    /// Converts IModelObject into RdfNode with all 
    /// the properties turned into RdfTriples
    /// </summary>
    /// <param name="modelObject">Rdf triple object - CIM object.</param>
    /// <returns>Converted RdfNode or null.</returns>
    private RdfNode? ModelObjectToRdfNode(IModelObject modelObject)
    {
        var metaClass = modelObject.MetaClass;

        var rdfNode = new RdfNode(GetBasedIdentifier(modelObject.Uuid, 
            IdentifierPrefix),
            metaClass.BaseUri,
            modelObject.IsAuto);

        foreach (var schemaProperty in metaClass.AllProperties)
        {
            WriteObjectProperty(rdfNode, modelObject, schemaProperty);
        }

        return rdfNode;
    }

    /// <summary>
    /// Converts properties into RdfNodes.
    /// </summary>
    /// <param name="modelObject">Rdf triple object - CIM object.</param>
    /// <param name="properties">CIM meta properties set.</param>
    /// <returns>Set of Rdf property triples.</returns>
    private void WriteObjectProperty(RdfNode objectNode, 
        IModelObject modelObject,
        ICimMetaProperty property)
    {
        if (modelObject.MetaClass.HasProperty(property) == false)
        {
            return;
        }

        object? objectData = null;
        switch (property.PropertyKind)
        {
            case CimMetaPropertyKind.Attribute:
                objectData = GetObjectAsAttribute(modelObject, property);
                break;
            case CimMetaPropertyKind.Assoc1To1:
                objectData = GetObjectAsAssoc1To1(modelObject, property);
                break;
            case CimMetaPropertyKind.Assoc1ToM:
                objectData = GetObjectAsAssoc1ToM(modelObject, property);
                break;
            //CimMetaPropertyKind.NonStandard
            default: 
                break;
        }

        if (objectData is IEnumerable<Uri> tripleObjects)
        {
            tripleObjects.ToList().ForEach(to => 
                objectNode.NewTriple(property.BaseUri, to));
        }
        else if (objectData is not null)
        {
            objectNode.NewTriple(property.BaseUri, objectData);
        }
    }

    /// <summary>
    /// Converts attribute property to RdfTriple.
    /// </summary>
    /// <param name="subject">Rdf triple subject - CIM object.</param>
    /// <param name="attribute"></param>
    /// <returns></returns>
    private object? GetObjectAsAttribute(IModelObject subject, 
        ICimMetaProperty attribute)
    {
        object? tripleObject = null;
        if (attribute.PropertyDatatype is ICimMetaDatatype datatype)
        {
            tripleObject = subject.GetAttribute(attribute);
        }
        else if (attribute.PropertyDatatype is ICimMetaClass metaClass)
        {
            if (metaClass.IsEnum)
            {
                var enumObject = subject.GetAttribute(attribute);
                if (enumObject is Uri)
                {
                    tripleObject = enumObject as Uri;
                }
                else if (enumObject is Enum typedEnum)
                {   
                    tripleObject = Schema.GetClassIndividuals(metaClass)
                        .Where(i => i.ShortName == typedEnum.ToString())
                        .Select(i => i.BaseUri)
                        .FirstOrDefault();
                }
            }

            if (metaClass.IsCompound)
            {
                var compoundObject = subject.GetAttribute<IModelObject>(attribute);
                    
                if (compoundObject != null)
                {
                    tripleObject = ModelObjectToRdfNode(compoundObject);
                }   
            }
        }

        return tripleObject;
    }

    /// <summary>
    /// Converts Assoc1To1 property to RdfTriple.
    /// </summary>
    /// <param name="subject">Rdf triple subject - CIM object.</param>
    /// <param name="assoc1To1">CIM meta property - assoc.</param>
    /// <returns>Rdf property triple.</returns>
    private object? GetObjectAsAssoc1To1(IModelObject subject, 
        ICimMetaProperty assoc1To1)
    {
        object resultAssocObject;

        var assocObj = subject.GetAssoc1To1<IModelObject>(assoc1To1);
        if (assocObj == null)
        {
            return null;
        }

        if (assocObj.MetaClass.IsCompound)
        {
            var compoundNode = ModelObjectToRdfNode(assocObj);
            if (compoundNode == null)
            {
                throw new Exception("RdfXmlSerializer.GetObjectAsAssoc1To1 compound null");
            }

            resultAssocObject = compoundNode;
        }
        else
        {
            resultAssocObject = GetBasedIdentifier(assocObj.Uuid,
                IdentifierPrefix);
        }

        return resultAssocObject;
    }

    /// <summary>
    /// Converts Assoc1ToM property to RdfTriple collection
    /// </summary>
    /// <param name="subject">Rdf triple subject - CIM object.</param>
    /// <param name="assoc1ToM">CIM meta property - assoc.</param>
    /// <returns>Set of Rdf property triples.</returns>
    private IEnumerable<Uri> GetObjectAsAssoc1ToM(IModelObject subject,
        ICimMetaProperty assoc1ToM)
    {
        return subject.GetAssoc1ToM(assoc1ToM)
            .Select(mo => GetBasedIdentifier(mo.Uuid, IdentifierPrefix));
    }

    /// <summary>
    /// Make URI from uuid and provider source path.
    /// </summary>
    /// <param name="uuid">IModelObject uuid.</param>
    /// <param name="prefix">Uuid prefix. 
    /// '#_' for rdf:about, '_' for rdf:id.</param>
    /// <returns></returns>
    private Uri GetBasedIdentifier(string uuid, string prefix)
    {
        return new(_RdfWriter.Namespaces["base"] + $"{prefix}{uuid}");
    }

    /// <summary>
    /// Fill writer namespaces from CimSchema.
    /// </summary>
    private void InitializeWriterNamespaces()
    {
        if (_RdfWriter == null)
        {
            return;
        }

        foreach (var ns in Schema.Namespaces)
        {
            if (ns.Key == "base")
            {
                continue;
            }

            _RdfWriter.AddNamespace(ns.Key, ns.Value);
        }

        _RdfWriter.AddNamespace("base", BaseUri);
    }

    #endregion

    #region DeserializerBlock

    private void InitializeRdfReader(StreamReader streamReader)
    {
        if (streamReader == null
            || streamReader.BaseStream.CanSeek == false)
        {
            throw new Exception("No data stream for read!");
        }

        _streamReader = streamReader;
        _RdfReader.Load(streamReader);
        _RdfReader.AddNamespace("base", BaseUri);
    }

    private void ResetCache()
    {        
        _objectsCache = [];
        _waitingReferenceObjectUuids = [];
    }

    /// <summary>
    /// Convert RdfNode objects to IModelObject collection.
    /// </summary>
    /// <returns>Collection of IModelObject instances.</returns>
    private List<IModelObject> ReadObjects()
    {
        ResetCache();

        if (_streamReader == null)
        {
            throw new Exception("Serializer reader has not been intialized!");
        }

        // First step - creating objects.
        foreach (var instanceNode in _RdfReader.ReadAll())
        {
            var instance = RdfNodeToModelObject(instanceNode);
            if (instance == null)
            {
                continue;
            }

            _objectsCache.TryAdd(instance.Uuid, instance);
        }

        // Second step - fill objects properties.
        _streamReader.BaseStream.Position = 0;
        _streamReader.DiscardBufferedData();    
        InitializeRdfReader(_streamReader);
        foreach (var instanceNode in _RdfReader.ReadAll())
        {
            if (RdfUtils.TryGetEscapedIdentifier(instanceNode.Identifier,
                out var instanceUuid) == false)
            {
                continue;
            }

            if (_objectsCache.TryGetValue(instanceUuid,
                out var instance) == false)
            {
                continue;
            }

            foreach (var property in instanceNode.Triples)
            {
                ReadObjectProperty(instance, property);
            }
        }

        _RdfReader.Close();
        _streamReader.Close();

        return [.. _objectsCache.Values];
    }

    /// <summary>
    /// Build IModelObject instance from RdfNode.
    /// </summary>
    /// <param name="instanceNode">RdfNode CIM object presentation.</param>
    /// <param name="IsCompound">Is compound (inner child) node.</param>
    /// <returns>IModelObject instance or null.</returns>
    private IModelObject? RdfNodeToModelObject(RdfNode instanceNode)
    {
        if (RdfUtils.TryGetEscapedIdentifier(instanceNode.Identifier,
            out var instanceUuid) == false)
        {
            return null;
        }

        var metaClass = Schema.TryGetResource<ICimMetaClass>
            (instanceNode.TypeIdentifier);

        if (metaClass == null)
        {
            return null;
        }

        IModelObject? instanceObject = null;

        if (TypeLib.RegisteredTypes.TryGetValue(instanceNode.TypeIdentifier,
            out var type))
        {
            instanceObject = Activator.CreateInstance(type, 
                instanceUuid, metaClass, instanceNode.IsAuto) as IModelObject;
        }
        else
        {
            instanceObject = new ModelObject(instanceUuid, 
                metaClass, instanceNode.IsAuto);
        }

        return instanceObject;
    }

    /// <summary>
    /// Convert RDF n-triple to IModelObject CIM property.
    /// </summary>
    /// <param name="instance">IModelObject CIM class instance.</param>
    /// <param name="propertyTriple">RDF n-triple.</param>
    private void ReadObjectProperty(IModelObject instance,
        RdfTriple propertyTriple)
    {
        var schemaProperty = Schema.TryGetResource<ICimMetaProperty>
            (propertyTriple.Predicate);

        if (schemaProperty == null)
        {
            return;
        }

        object? data = DeserializableDataSelector(propertyTriple.Object);
        if (data == null)
        {
            return;
        }

        switch (schemaProperty.PropertyKind)
        {
            case CimMetaPropertyKind.Attribute:
            {
                SetObjectDataAsAttribute(instance,
                    schemaProperty, data);
                break;
            }
            case CimMetaPropertyKind.Assoc1To1:
            case CimMetaPropertyKind.Assoc1ToM:
            {
                SetObjectDataAsAssociation(instance,
                    schemaProperty, (Uri)data);
                break;
            }
        }
    }

    /// <summary>
    /// Select expecting IModelObject property data.
    /// </summary>
    /// <returns>Casted data or compound.</returns>
    private object? DeserializableDataSelector(object data)
    {
        if (data is RdfNode objectRdfNode)
        {
            return MakeCompoundPropertyObject(objectRdfNode);
        }
        else if (data is Uri objectUri)
        {
            return objectUri;
        }
        else
        {
            return data as string;
        }
    }

    /// <summary>
    /// Push attribute value to IModelObject.
    /// </summary>
    /// <param name="instance">IModelObject instance.</param>
    /// <param name="property">CIM meta property.</param>
    /// <param name="data">Attribute value: simple, enum or compound.</param>
    private void SetObjectDataAsAttribute(IModelObject instance,
        ICimMetaProperty property, object data)
    {
        object? endData = null;
        if (property.PropertyDatatype is ICimMetaDatatype dataType)
        {
            var convertedValue = Convert.ChangeType(data,
                dataType.PrimitiveType, CultureInfo.InvariantCulture);

            if (convertedValue != null)
            {
                endData = convertedValue;
            }
        }
        else if (property.PropertyDatatype is ICimMetaClass dataClass)
        {
            if (dataClass.IsCompound && data is IModelObject dataObject)
            {
                endData = dataObject;
            }
            else if (dataClass.IsEnum
                && data is Uri enumValueUri)
            {
                var schemaEnumValue = Schema
                    .TryGetResource<ICimMetaIndividual>(enumValueUri);

                if (schemaEnumValue != null)
                {
                    if (TypeLib.RegisteredTypes.TryGetValue(
                        property.PropertyDatatype.BaseUri, out var typeEnum))
                    {
                        var enumValue = Enum.Parse(typeEnum,
                            schemaEnumValue!.ShortName);

                        endData = enumValue;
                    }
                    else
                    {
                        endData = enumValueUri;
                    }
                }
                else
                {
                    throw new Exception($"Enum value {enumValueUri} does not exist in schema!");
                }
            }
        }

        try
        { 
            if (endData != null)
            {
                instance.SetAttribute(property, endData);
            }
        }
        catch (Exception ex)
        {
            _Log.NewMessage($"Failed set attribute to instance {instance.Uuid}", 
                LogMessageSeverity.Error, ex.Message);
        }
    }

    /// <summary>
    /// Push association to IModelObject.
    /// </summary>
    /// <param name="instance">IModelObject instance.</param>
    /// <param name="property">CIM meta property.</param>
    /// <param name="referenceUri">Association object reference by Uri.</param>
    private void SetObjectDataAsAssociation(IModelObject instance,
        ICimMetaProperty property, Uri referenceUri)
    {
        string referenceUuid = string.Empty;
        if (RdfUtils.TryGetEscapedIdentifier(referenceUri,
            out referenceUuid) == false)
        {
            return;
        }

        _objectsCache.TryGetValue(referenceUuid, out var referenceInstance);

        if (referenceInstance == null)
        {
            referenceInstance = new ModelObjectUnresolvedReference
                (referenceUuid, instance.MetaClass);

            _waitingReferenceObjectUuids.Add(instance.Uuid);
        }

        try
        {
            if (property.PropertyKind == CimMetaPropertyKind.Assoc1To1)
            {
                instance.SetAssoc1To1(property,
                    referenceInstance);
            }
            else if (property.PropertyKind == CimMetaPropertyKind.Assoc1ToM)
            {
                instance.AddAssoc1ToM(property,
                    referenceInstance);
            }
        }
        catch (Exception ex)
        {
            _Log.NewMessage($"Failed set association to instance {instance.Uuid}", 
                LogMessageSeverity.Error, ex.Message);
        }
    }

    /// <summary>
    /// Makes auto compound object from RdfNode.
    /// </summary>
    /// <param name="objectRdfNode">Compound property Rdf node.</param>
    /// <returns>Auto IModelObject or null.</returns>
    private IModelObject? MakeCompoundPropertyObject(RdfNode objectRdfNode)
    {
        var compoundPropertyObject = RdfNodeToModelObject(objectRdfNode);
        if (compoundPropertyObject == null)
        {
            return null;
        }

        foreach (var property in objectRdfNode.Triples)
        {
            ReadObjectProperty(compoundPropertyObject, property);
        }

        return compoundPropertyObject;
    }

    #endregion

    private ICimSchema _schema;
    private ICimDatatypeLib _typeLib;

    private Dictionary<string, IModelObject> _objectsCache = [];
    private HashSet<string> _waitingReferenceObjectUuids = [];

    private StreamReader? _streamReader;
    private StreamWriter? _streamWriter;

    private PlainLogView _Log;
}
