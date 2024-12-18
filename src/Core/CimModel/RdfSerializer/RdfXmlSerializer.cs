using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.DataProvider;
using CimBios.Core.RdfIOLib;
using System.Globalization;
using System.Xml.Linq;

namespace CimBios.Core.CimModel.RdfSerializer;

/// <summary>
/// CIM Rdf/Xml serializer implementation. Based on RdfXmlIOLib.
/// </summary>
public class RdfXmlSerializer : RdfSerializerBase
{
    public RdfXmlSerializer(RdfXmlFileDataProvider provider,
        ICimSchema schema, IDatatypeLib datatypeLib)
        : base(provider, schema, datatypeLib)
    {
        _objectsCache = [];
        _waitingReferenceObjectUuids = [];
        _checkedPropsCache = [];
    }

    public override IEnumerable<IModelObject> Deserialize()
    {
        _reader = new RdfXmlReader(Provider.Source);
        if (Provider.Get() is XDocument xDocument)
        {
            _reader.Load(xDocument);
        }

        var deserializedObjects = ReadObjects();
        ResetCache();

        return deserializedObjects;
    }

    public override void Serialize(IEnumerable<IModelObject> modelObjects)
    {
        _writer = new RdfXmlWriter();
        InitializeWriterNamespaces();

        var serializedObjects = WriteObjects(modelObjects);
        Provider.Push(serializedObjects);

        ResetCache();
    }

    #region SerializerBlock

    private XDocument WriteObjects(IEnumerable<IModelObject> modelObjects)
    {
        if (_writer == null)
        {
            throw new Exception("Writter was not initialized!");
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

        return _writer.Write(objsToWrite);
    }

    /// <summary>
    /// Converts IModelObject into RdfNode with all 
    /// the properties turned into RdfTriples
    /// </summary>
    /// <param name="modelObject">Rdf triple object - CIM object.</param>
    /// <returns>Converted RdfNode or null.</returns>
    private RdfNode? ModelObjectToRdfNode(IModelObject modelObject)
    {
        var metaClass = Schema.TryGetResource<ICimMetaClass>(
            modelObject.ObjectData.ClassType);

        if (metaClass == null)
        {
            return null;
        }

        var rdfNode = new RdfNode(GetBasedIdentifier(modelObject.Uuid, 
            IdentifierPrefix),
            metaClass.BaseUri,
            modelObject.ObjectData.IsAuto);

        var schemaProperties = Schema.GetClassProperties(metaClass, true);

        foreach (var schemaProperty in schemaProperties)
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
        if (modelObject.ObjectData.HasProperty(property.ShortName) == false)
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

    /// TODO:   Extract type parsing in the other one method.
    ///         Fix datatypelib enum handling - need parse uri from schema by enum value.
    ///         + code style %)
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
        if (attribute.PropertyDatatype is ICimMetaDatatype metaDatatype)
        {
            tripleObject = subject.ObjectData
                .GetAttribute(attribute.ShortName, metaDatatype.PrimitiveType);
        }
        else if (attribute.PropertyDatatype is ICimMetaClass metaClass)
        {
            if (TypeLib.RegisteredTypes.TryGetValue(metaClass.BaseUri, 
                out var libType))
            {
                tripleObject = subject.ObjectData
                    .GetAttribute(attribute.ShortName, libType);
            }

            if (metaClass.IsEnum)
            {
                tripleObject = subject.ObjectData
                    .GetAttribute(attribute.ShortName, typeof(Uri));
            }

            if (metaClass.IsCompound)
            {
                var compoundObject = subject.ObjectData
                    .GetAttribute<IModelObject>(attribute.ShortName);

                tripleObject = ModelObjectToRdfNode(compoundObject);
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

        var assocObj = subject.ObjectData.GetAssoc1To1(assoc1To1.ShortName);
        if (assocObj.ObjectData.IsAuto)
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
        return subject.ObjectData.GetAssoc1ToM(assoc1ToM.ShortName)
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
        return new(Provider.Source + $"{prefix}{uuid}");
    }

    /// <summary>
    /// Fill writer namespaces from CimSchema.
    /// </summary>
    private void InitializeWriterNamespaces()
    {
        if (_writer == null)
        {
            return;
        }

        foreach (var ns in Schema.Namespaces)
        {
            if (ns.Key == "base")
            {
                continue;
            }

            _writer.AddNamespace(ns.Key, ns.Value);
        }

        _writer.AddNamespace("base", Provider.Source);
    }

    #endregion

    #region DeserializerBlock

    private void ResetCache()
    {        
        _objectsCache = [];
        _waitingReferenceObjectUuids = [];
        _checkedPropsCache = [];
    }

    /// <summary>
    /// Convert RdfNode objects to IModelObject collection.
    /// </summary>
    /// <returns>Collection of IModelObject instances.</returns>
    private List<IModelObject> ReadObjects()
    {
        _objectsCache.Clear();
        _waitingReferenceObjectUuids.Clear();

        if (_reader == null)
        {
            throw new Exception("Reader was not initialized!");
        }

        var cimDocument = _reader.ReadAll();
        // First step - creating objects.
        foreach (var instanceNode in cimDocument)
        {
            var instance = RdfNodeToModelObject(instanceNode, false);
            if (instance == null)
            {
                continue;
            }

            _objectsCache.TryAdd(instance.Uuid, instance);
        }

        // Second step - fill objects properties.
        foreach (var instanceNode in cimDocument)
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

        _reader.Close();

        return [.. _objectsCache.Values];
    }

    /// <summary>
    /// Build IModelObject instance from RdfNode.
    /// </summary>
    /// <param name="instanceNode">RdfNode CIM object presentation.</param>
    /// <param name="IsCompound">Is compound (inner child) node.</param>
    /// <returns>IModelObject instance or null.</returns>
    private IModelObject? RdfNodeToModelObject(RdfNode instanceNode,
        bool IsCompound = false)
    {
        if (RdfUtils.TryGetEscapedIdentifier(instanceNode.Identifier,
            out var instanceUuid) == false)
        {
            return null;
        }

        if (Schema.TryGetResource<ICimMetaClass>
            (instanceNode.TypeIdentifier) == null)
        {
            return null;
        }

        DataFacade objectData = new DataFacade(
            instanceUuid,
            instanceNode.TypeIdentifier,
            instanceNode.IsAuto,
            IsCompound);

        IModelObject? instanceObject = null;

        if (TypeLib.RegisteredTypes.TryGetValue(instanceNode.TypeIdentifier,
            out var type))
        {
            instanceObject = Activator.CreateInstance(type, objectData)
                as IModelObject;
        }
        else
        {
            instanceObject = new ModelObject(objectData);
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

        if (schemaProperty == null
            || IsPropertyAlignSchemaClass(schemaProperty, instance) == false)
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
            if (dataClass.IsCompound
                && data is IModelObject dataObject)
            {
                bool isClassesMatches = dataClass.BaseUri.AbsoluteUri
                    == dataObject.ObjectData.ClassType.AbsoluteUri;

                if (isClassesMatches)
                {
                    endData = dataObject;
                }
            }
            else if (dataClass.IsEnum
                && data is Uri enumValueUri)
            {
                var schemaEnumValue = Schema
                    .TryGetResource<ICimMetaIndividual>(enumValueUri);

                bool isClassesMatches = Schema
                    .GetClassIndividuals(dataClass, true)
                    .Contains(schemaEnumValue);

                var a = Schema
                    .GetClassIndividuals(dataClass, true);

                if (isClassesMatches)
                {
                    if (TypeLib.RegisteredTypes.TryGetValue(
                            schemaEnumValue!.InstanceOf!.BaseUri,
                            out var typeEnum))
                    {
                        var enumValue = Enum.Parse(typeEnum,
                            schemaEnumValue.ShortName);

                        endData = enumValue;
                    }
                    else
                    {
                        endData = enumValueUri;
                    }
                }
            }
        }

        if (endData != null)
        {
            instance.ObjectData.SetAttribute(
                property.ShortName,
                endData);
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

        if (property.PropertyDatatype is ICimMetaClass assocClassType == false)
        {
            return;
        }

        IModelObject? referenceInstance = null;
        if (_objectsCache.TryGetValue(referenceUuid, out var modelObject))
        {
            var referenceMetaClass = Schema.TryGetResource<ICimMetaClass>
                (modelObject.ObjectData.ClassType);

            if (referenceMetaClass == null)
            {
                return;
            }

            if (referenceMetaClass == assocClassType
                || referenceMetaClass.AllAncestors.Any(a =>
                    a.BaseUri.AbsoluteUri == assocClassType.BaseUri.AbsoluteUri)
            )
            {
                referenceInstance = modelObject;
            }
            else
            {
                return;
            }
        }

        if (referenceInstance == null)
        {
            referenceInstance =
                new ModelObjectUnresolvedReference(
                    new DataFacade(referenceUuid,
                        property.BaseUri)
                );

            _waitingReferenceObjectUuids.Add(instance.Uuid);
        }

        if (property.PropertyKind == CimMetaPropertyKind.Assoc1To1)
        {
            instance.ObjectData.SetAssoc1To1(property.ShortName,
                referenceInstance);
        }
        else if (property.PropertyKind == CimMetaPropertyKind.Assoc1ToM)
        {
            instance.ObjectData.AddAssoc1ToM(property.ShortName,
                referenceInstance);
        }
    }

    /// <summary>
    /// Makes auto compound object from RdfNode.
    /// </summary>
    /// <param name="objectRdfNode">Compound property Rdf node.</param>
    /// <returns>Auto IModelObject or null.</returns>
    private IModelObject? MakeCompoundPropertyObject(RdfNode objectRdfNode)
    {
        var compoundPropertyObject = RdfNodeToModelObject(objectRdfNode, true);
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

    /// <summary>
    /// Check is property consider schema.
    /// </summary>
    /// <param name="schemaProperty">CIM meta property.</param>
    /// <param name="instance">IModelObject instance.</param>
    private bool IsPropertyAlignSchemaClass(ICimMetaProperty schemaProperty,
        IModelObject instance)
    {
        if (IsPropertyChecked(instance.ObjectData.ClassType, 
            schemaProperty.BaseUri))
        {
            return true;
        }

        if (schemaProperty == null
            || schemaProperty.OwnerClass == null)
        {
            return false;
        }

        var instanceClass = Schema.TryGetResource<ICimMetaClass>
            (instance.ObjectData.ClassType);
        if (instanceClass == null)
        {
            return false;
        }

        var classProperties = Schema.GetClassProperties(instanceClass, true);
        if (classProperties.Contains(schemaProperty))
        {
            CacheCheckedProperty(instanceClass.BaseUri, 
                schemaProperty.BaseUri);

            return true;
        }

        return false;
    }

    private bool IsPropertyChecked(Uri classType, Uri classProperty)
    {
        var classTypeId = classType.AbsoluteUri;
        var classPropertyId = classProperty.AbsoluteUri;

        if (_checkedPropsCache.TryGetValue(classTypeId, out var props)
            && props.Contains(classPropertyId))
        {
            return true;
        } 

        return false;
    }

    private void CacheCheckedProperty(Uri classType, Uri classProperty)
    {
        var classTypeId = classType.AbsoluteUri;
        var classPropertyId = classProperty.AbsoluteUri;

        if (_checkedPropsCache.ContainsKey(classTypeId) == false)
        {
            _checkedPropsCache.Add(classTypeId, []);
        }
        
        _checkedPropsCache[classTypeId].Add(classPropertyId);
    }

    #endregion

    private RdfXmlReader? _reader;
    private RdfXmlWriter? _writer;

    private Dictionary<string, IModelObject> _objectsCache;
    private HashSet<string> _waitingReferenceObjectUuids;
    private Dictionary<string, List<string>> _checkedPropsCache;
}
