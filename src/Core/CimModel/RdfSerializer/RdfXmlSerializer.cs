using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.DataProvider;
using CimBios.Core.RdfXmlIOLib;
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
        _writer = new RdfXmlWriter();

        var objsToWrite = new List<RdfNode>();
        foreach (var modelObject in modelObjects)
        {
            objsToWrite.Add(ProcessObject(modelObject));
        }

        XDocument writtenObjects = _writer.Write(objsToWrite);
        if (writtenObjects != null)
        {
            Provider.Push(writtenObjects);
        }
    }

    private RdfNode ProcessObject(IModelObject modelObject)
    {
        var triples = new List<RdfTriple>();
        foreach (var property in modelObject.ObjectData.Attributes)
        {
            triples.Add(new RdfTriple(new Uri(modelObject.Uuid), new Uri(property), modelObject.ObjectData.GetAttribute<string>(property)));
        }
        foreach (var property in modelObject.ObjectData.Assocs1To1)
        {
            if (modelObject.ObjectData.GetAssoc1To1(property).ObjectData.IsCompound) // ���������, ��� ���� ��� isCompound
            {
                var compound = modelObject.ObjectData.GetAssoc1To1(property);
                var compoundNode = ProcessObject(compound); // ��������
                triples.Add(new RdfTriple(new Uri(modelObject.Uuid), new Uri(property), compoundNode));
            }
            triples.Add(new RdfTriple(new Uri(modelObject.Uuid), new Uri(property), new Uri(modelObject.ObjectData.GetAssoc1To1(property).Uuid)));
        }
        foreach (var property in modelObject.ObjectData.Assocs1ToM)
        {
            if (modelObject.ObjectData.GetAssoc1ToM(property).Any())
            {
                foreach (ModelObject refObj in modelObject.ObjectData.GetAssoc1ToM(property))
                {
                    triples.Add(new RdfTriple(new Uri(modelObject.Uuid), new Uri(property), new Uri(refObj.Uuid)));
                }
            }
        }
        var triplesArr = triples.ToArray();
        return new RdfNode(new Uri(modelObject.Uuid), modelObject.ObjectData.ClassType, triplesArr, modelObject.ObjectData.IsAuto);
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
                    .TryGetDescription<ICimMetaInstance>(enumValueUri);

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
                .Any(a => a.BaseUri.AbsoluteUri == schemaPropClassUri) )
        {
            return true;
        }

        return false;
    }

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

    private RdfXmlIOLib.RdfXmlReader? _reader;
    private RdfXmlIOLib.RdfXmlWriter? _writer;

    private Dictionary<string, IModelObject> _objectsCache;
    private HashSet<string> _waitingReferenceObjectUuids;
}
