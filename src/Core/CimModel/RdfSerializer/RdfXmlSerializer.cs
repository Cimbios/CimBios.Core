using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.DataProvider;
using CimBios.Core.RdfXmlIOLib;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
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
        _objectsCache = new Dictionary<string, IModelObject>();
        _waitingReferenceObjectUuids = new HashSet<string>();
    }

    public override IEnumerable<IModelObject> Deserialize()
    {
        _reader = new RdfXmlReader(Provider.Source.AbsoluteUri);
        if (Provider.Get() is XDocument xDocument)
        {
            _reader.Load(xDocument);
        }

        return ReadObjects();
    }

    public override void Serialize(IEnumerable<IModelObject> modelObjects)
    {
        _writer = new RdfXmlWriter((Dictionary<string, Uri>)Schema.Namespaces);

        var objsToWrite = new List<RdfNode>();
        foreach (var modelObject in modelObjects)
        {
            objsToWrite.Add(ProcessObject(modelObject));
        }
        if (_writer.Write(objsToWrite) is XDocument writtenObjects)
        {
            Provider.Push(writtenObjects);
        }
    }

    /// <summary>
    /// Converts IModelObject into RdfNode with all the properties turned into RdfTriples
    /// </summary>
    /// <param name="modelObject"></param>
    /// <returns></returns>
    /// TODO:   schema support; | Done
    ///         split writing on attribute and assoc methods; | Done
    ///         check nullity of object properties; | Questions
    ///         impl identifier maker method.| Questions
    private RdfNode ProcessObject(IModelObject modelObject)
    {
        var objProperties = Schema.GetClassProperties(GetObjectClass(modelObject), true);
        var triples = WriteProperties(modelObject, objProperties);
        return new RdfNode(new Uri(modelObject.ObjectData.ClassType.ToString() + $"|{modelObject.Uuid}"),
                           GetObjectClass(modelObject).BaseUri,
                           triples,
                           modelObject.ObjectData.IsAuto);
    }

    /// <summary>
    /// Converts properties into RdfNodes
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="properties"></param>
    /// <returns></returns>
    private RdfTriple[] WriteProperties(IModelObject obj,
                                        IEnumerable<ICimMetaProperty> properties)
    {
        var result = new List<RdfTriple>();
        foreach (var property in properties)
        {
            // TODO: Check if Property has value and not null
            if (!obj.ObjectData.HasProperty(property.ShortName))
            {
                continue;
            }

            
            switch (property.PropertyKind)
            {
                case CimMetaPropertyKind.Attribute:
                    var attributeResult = WriteAttribute(obj, property);
                    if (attributeResult != null)
                    {
                        result.Add(attributeResult);
                    }
                    break;
                case CimMetaPropertyKind.Assoc1To1:
                    result.Add(WriteAssoc1To1(obj, property));
                    break;
                case CimMetaPropertyKind.Assoc1ToM:
                    result.AddRange(WriteAssoc1ToM(obj, property));
                    break;
                default: //CimMetaPropertyKind.NonStandard
                    break;
            }
        }
        return result.ToArray();
    }

    /// <summary>
    /// Converts attribute property to RdfTriple
    /// </summary>
    /// <param name="subj"></param>
    /// <param name="attribute"></param>
    /// <returns></returns>
    private RdfTriple WriteAttribute(IModelObject subj, ICimMetaProperty attribute)
    {
        //ICimMetaDatatype? datatype = attribute.PropertyDatatype as ICimMetaDatatype;

        Type? simpleType = null;

        if (attribute.PropertyDatatype is ICimMetaDatatype metaDatatype)
        {
            simpleType = metaDatatype.SimpleType;
        }
        else if(attribute.PropertyDatatype is ICimMetaClass metaClass)
        {

            simpleType = typeof(ModelObject);
            if (TypeLib.RegisteredTypes.TryGetValue(metaClass.BaseUri, out var libType))
            {
                simpleType = libType;
            }
            if (metaClass.IsEnum)
            {
                simpleType = typeof(Uri);
            }
        }

        if (simpleType != null)
        {
            return new RdfTriple(new Uri(subj.ObjectData.ClassType.ToString() + $"|{subj.Uuid}"),
                                         attribute.BaseUri,
                                         subj.ObjectData.GetAttribute(attribute.ShortName, simpleType));
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Converts Assoc1To1 property to RdfTriple
    /// </summary>
    /// <param name="subj"></param>
    /// <param name="assoc"></param>
    /// <returns></returns>
    private RdfTriple WriteAssoc1To1(IModelObject subj, ICimMetaProperty assoc1To1)
    {
        var assocObj = subj.ObjectData.GetAssoc1To1(assoc1To1.ShortName);
        if (assocObj.ObjectData.IsAuto)
        {
            return new RdfTriple(new Uri(subj.ObjectData.ClassType.ToString() + $"|{subj.Uuid}"),
                                 assoc1To1.BaseUri,
                                 ProcessObject(assocObj));
        }
        else
        {
            return new RdfTriple(new Uri(subj.ObjectData.ClassType.ToString() + $"|{subj.Uuid}"),
                                 assoc1To1.BaseUri,
                                 assocObj.Uuid);
        }
    }

    /// <summary>
    /// Converts Assoc1ToM property to RdfTriple collection
    /// </summary>
    /// <param name="subj"></param>
    /// <param name="assoc"></param>
    /// <returns></returns>
    private IEnumerable<RdfTriple> WriteAssoc1ToM(IModelObject subj,
                                                  ICimMetaProperty assoc1ToM)
    {
        var triples = new List<RdfTriple>();
        foreach (IModelObject obj in subj.ObjectData.GetAssoc1ToM(assoc1ToM.ShortName))
        {
            triples.Add(new RdfTriple(new Uri(subj.ObjectData.ClassType.ToString() + $"|{subj.Uuid}"),
                                      assoc1ToM.BaseUri,
                                      obj.Uuid));
        }
        return triples;
    }

    /// <summary>
    /// Finds object's class in schema
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private ICimMetaClass GetObjectClass(IModelObject obj)
    {
        return Schema.Classes.Where(x => RdfXmlReaderUtils.RdfUriEquals(x.BaseUri, obj.ObjectData.ClassType)).FirstOrDefault();
    }

    /// <summary>
    /// Convert RdfNode objects to IModelObject collection.
    /// </summary>
    /// <returns>Collection of IModelObject instances.</returns>
    private IEnumerable<IModelObject> ReadObjects()
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
            var instance = CreateInstance(instanceNode, false);
            if (instance == null)
            {
                continue;
            }

            _objectsCache.TryAdd(instance.Uuid, instance);
        }

        // Second step - fill objects properties.
        foreach (var instanceNode in cimDocument)
        {
            if (TryGetEscapedIdentifier(instanceNode.Identifier,
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
                InitializeObjectProperty(instance, property);
            }
        }

        _reader.Close();

        return _objectsCache.Values.ToList();
    }

    /// <summary>
    /// Build IModelObject instance from RdfNode.
    /// </summary>
    /// <param name="instanceNode">RdfNode CIM object presentation.</param>
    /// <param name="IsCompound">Is compound (inner child) node.</param>
    /// <returns>IModelObject instance or null.</returns>
    private IModelObject? CreateInstance(RdfNode instanceNode,
        bool IsCompound = false)
    {
        if (TryGetEscapedIdentifier(instanceNode.Identifier,
            out var instanceUuid) == false)
        {
            return null;
        }

        if (Schema.TryGetDescription<ICimMetaClass>
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
    private void InitializeObjectProperty(IModelObject instance,
        RdfTriple propertyTriple)
    {
        var schemaProperty = Schema.TryGetDescription<ICimMetaProperty>
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
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="property"></param>
    /// <param name="data"></param>
    private void SetObjectDataAsAttribute(IModelObject instance,
        ICimMetaProperty property, object data)
    {
        object? endData = null;
        if (property.PropertyDatatype is ICimMetaDatatype dataType)
        {
            var convertedValue = Convert.ChangeType(data,
                dataType.SimpleType, CultureInfo.InvariantCulture);

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
                    .TryGetDescription<ICimMetaIndividual>(enumValueUri);

                bool isClassesMatches = schemaEnumValue?.InstanceOf?
                    .BaseUri.AbsoluteUri == dataClass.BaseUri.AbsoluteUri;

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
    /// 
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="property"></param>
    /// <param name="referenceUri"></param>
    private void SetObjectDataAsAssociation(IModelObject instance,
        ICimMetaProperty property, Uri referenceUri)
    {
        string referenceUuid = string.Empty;
        if (TryGetEscapedIdentifier(referenceUri,
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
            var referenceMetaClass = Schema.TryGetDescription<ICimMetaClass>
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
    /// 
    /// </summary>
    /// <param name="objectRdfNode"></param>
    /// <returns></returns>
    private IModelObject? MakeCompoundPropertyObject(RdfNode objectRdfNode)
    {
        var compoundPropertyObject = CreateInstance(objectRdfNode, true);
        if (compoundPropertyObject == null)
        {
            return null;
        }

        foreach (var property in objectRdfNode.Triples)
        {
            InitializeObjectProperty(compoundPropertyObject, property);
        }

        return compoundPropertyObject;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="schemaProperty"></param>
    /// <param name="instance"></param>
    /// <returns></returns>
    private bool IsPropertyAlignSchemaClass(ICimMetaProperty schemaProperty,
        IModelObject instance)
    {
        if (schemaProperty == null
            || schemaProperty.OwnerClass == null)
        {
            return false;
        }

        var instanceClass = Schema.TryGetDescription<ICimMetaClass>
            (instance.ObjectData.ClassType);
        if (instanceClass == null)
        {
            return false;
        }

        var schemaPropClassUri = schemaProperty.OwnerClass.BaseUri.AbsoluteUri;
        var instanceClassUri = instanceClass.BaseUri.AbsoluteUri;

        if (schemaPropClassUri == instanceClassUri
            || instanceClass.AllAncestors
                .Any(a => a.BaseUri.AbsoluteUri == schemaPropClassUri))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="identifier"></param>
    /// <returns></returns>
    private static bool TryGetEscapedIdentifier(Uri uri, out string identifier)
    {
        identifier = string.Empty;

        if (uri.Fragment != string.Empty)
        {
            identifier = uri.Fragment
                .Replace("#", "")
                .Replace("_", "");

            return true;
        }
        else if (uri.LocalPath != string.Empty)
        {
            identifier = uri.LocalPath.Replace("/", "");
            return true;
        }

        return false;
    }

    private RdfXmlReader? _reader;
    private RdfXmlWriter? _writer;

    private Dictionary<string, IModelObject> _objectsCache;
    private HashSet<string> _waitingReferenceObjectUuids;
}
