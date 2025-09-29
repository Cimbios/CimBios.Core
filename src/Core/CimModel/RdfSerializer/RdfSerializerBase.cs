using System.Globalization;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.Schema.AutoSchema;
using CimBios.Core.RdfIOLib;
using CimBios.Utils.ClassTraits.CanLog;

namespace CimBios.Core.CimModel.RdfSerializer;

/// <summary>
///     Base serializer class provides (de)serialization functions.
/// </summary>
public abstract class RdfSerializerBase : ICanLog
{
    private const string RdfDescription = "http://www.w3.org/1999/02/22-rdf-syntax-ns#Description";

    protected RdfSerializerBase(ICimSchema schema, ICimDatatypeLib datatypeLib)
    {
        Schema = schema;
        TypeLib = datatypeLib;
        _log = new PlainLogView(this);

        Settings = new RdfSerializerSettings();
    }

    /// <summary>
    ///     Rdf serializer settings.
    /// </summary>
    public RdfSerializerSettings Settings { get; init; }

    /// <summary>
    ///     Cim schema rules.
    /// </summary>
    private ICimSchema Schema { get; }

    /// <summary>
    ///     CIM data types library for concrete typed instances creating.
    /// </summary>
    private ICimDatatypeLib TypeLib { get; }

    public Uri BaseUri { get; set; } = new(
        "http://cim.bios/serialization/rdf");


    public IReadOnlyCollection<ModelObjectUnresolvedReference>
        UnresolvedReferences => _waitingReferenceObjects.Values;

    /// <summary>
    ///     Rdf reader abstract entity.
    /// </summary>
    protected abstract RdfReaderBase RdfReader { get; }

    /// <summary>
    ///     Rdf writer abstract entity.
    /// </summary>
    protected abstract RdfWriterBase RdfWriter { get; }

    /// <summary>
    ///     OID Descriptor factory for producing model objects.
    /// </summary>
    protected abstract IOIDDescriptorFactory OidDescriptorFactory { get; }

    /// <summary>
    ///     Cache used namespaces while objects serializing.
    /// </summary>
    private bool CacheUsedWritingNamespaces { get; set; } = true;

    /// <summary>
    ///     Cached used namespaces.
    /// </summary>
    private HashSet<Uri> CachedNamespaces { get; } = [];

    public ILogView Log => _log;
    
    private readonly PlainLogView _log;

    private Dictionary<string, CimAutoClass> _createdAutoClassesCache = [];
    private Dictionary<string, CimAutoProperty> _createdAutoPropertiesCache = [];

    private Dictionary<IOIDDescriptor, IModelObject> _objectsCache = [];

    private StreamReader? _streamReader;
    private StreamWriter? _streamWriter;

    private Dictionary<(IOIDDescriptor, ICimMetaProperty),
        ModelObjectUnresolvedReference> _waitingReferenceObjects = [];

    /// <summary>
    ///     Deserialize data provider data to IModelObject instances.
    ///     <returns>Deserializer IModelObject collection.</returns>
    /// </summary>
    public IEnumerable<IModelObject> Deserialize(StreamReader streamReader)
    {
        ResetCache();
        InitializeRdfReader(streamReader);
        Schema.InvalidateAuto();
        var deserializedObjects = ReadObjects();

        return deserializedObjects;
    }

    /// <summary>
    ///     Serialize IModelObject instances to data provider source.
    ///     <param name="modelObjects">IModelObject collection for serialization.</param>
    /// </summary>
    public void Serialize(StreamWriter streamWriter,
        IEnumerable<IModelObject> modelObjects)
    {
        ResetCache();
        
        var objsToWrite = BuildObjectsToWrite(modelObjects);
        
        InitializeWriterNamespaces(CachedNamespaces);
        InitializeRdfWriter(streamWriter);
        
        RdfWriter.WriteAll(objsToWrite);
        RdfWriter.Close();
    }

    #region SerializerBlock

    private void InitializeRdfWriter(StreamWriter streamWriter)
    {
        _streamWriter = streamWriter ?? throw new Exception("No data stream for write!");

        RdfWriter.RdfIRIMode = Settings.WritingIRIMode;
        RdfWriter.Open(streamWriter);
    }

    private List<RdfNode> BuildObjectsToWrite(
        IEnumerable<IModelObject> modelObjects)
    {
        var objsToWrite = new List<RdfNode>();
        foreach (var modelObject in modelObjects)
        {
            var moNode = ModelObjectToRdfNode(modelObject);
            if (moNode == null) continue;

            objsToWrite.Add(moNode);
        }
        
        return objsToWrite;
    }
    
    /// <summary>
    ///     Converts IModelObject into RdfNode with all
    ///     the properties turned into RdfTriples
    /// </summary>
    /// <param name="modelObject">Rdf triple object - CIM object.</param>
    /// <returns>Converted RdfNode or null.</returns>
    private RdfNode? ModelObjectToRdfNode(IModelObject modelObject)
    {
        var metaClass = modelObject.MetaClass;

        if (Schema.TryGetResource<ICimMetaClass>(metaClass.BaseUri) == null
            && Settings.UnknownClassesAllowed == false)
        {
            _log.Error(
                $"Failed to write instance {modelObject.OID} to Rdf node: " +
                $"Property {metaClass.ShortName} does not exist in schema!",
                modelObject);

            return null;
        }

        var rdfNode = new RdfNode(
            modelObject.OID.AbsoluteOID,
            metaClass.BaseUri,
            modelObject.OID is AutoDescriptor);

        foreach (var schemaProperty in metaClass.AllProperties)
            try
            {
                WriteObjectProperty(rdfNode, modelObject, schemaProperty);
            }
            catch (Exception ex)
            {
                _log.Error(
                    $"Failed to write property {schemaProperty.ShortName} " +
                    $"of instance {modelObject.OID} to Rdf node: {ex.Message}",
                    modelObject);
            }

        if (CacheUsedWritingNamespaces) 
            CachedNamespaces.Add(rdfNode.TypeIdentifier);
        
        return rdfNode;
    }

    /// <summary>
    ///     Converts properties into RdfNodes.
    /// </summary>
    /// <param name="objectNode">Rdf triple object - CIM object.</param>
    /// <param name="modelObject"></param>
    /// <param name="property">CIM meta properties set.</param>
    /// <returns>Set of Rdf property triples.</returns>
    private void WriteObjectProperty(RdfNode objectNode,
        IModelObject modelObject,
        ICimMetaProperty property)
    {
        if (modelObject.MetaClass.HasProperty(property) == false) return;

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
            case CimMetaPropertyKind.Statements:
                objectData = GetObjectAsStatements(modelObject, property);
                break;
            //CimMetaPropertyKind.NonStandard
        }

        if (objectData is Uri uriReference)
        {
            objectNode.NewTriple(property.BaseUri,
                new RdfTripleObjectUriContainer(uriReference));
        }
        else if (objectData is IEnumerable<Uri> tripleObjects)
        {
            tripleObjects.ToList().ForEach(to =>
                objectNode.NewTriple(property.BaseUri,
                    new RdfTripleObjectUriContainer(to)));
        }
        else if (objectData is RdfNode compoundRdfNode)
        {
            objectNode.NewTriple(property.BaseUri,
                new RdfTripleObjectStatementsContainer([compoundRdfNode]));
        }
        else if (objectData is IEnumerable<RdfNode> statements)
        {
            objectNode.NewTriple(property.BaseUri,
                new RdfTripleObjectStatementsContainer(statements.ToArray()));
        }
        else if (objectData is not null)
        {
            var formatted = FormatLiteralValue(objectData);
            if (formatted != null)
                objectNode.NewTriple(property.BaseUri,
                    new RdfTripleObjectLiteralContainer(formatted));
        }
        
        if (CacheUsedWritingNamespaces) 
            CachedNamespaces.Add(property.BaseUri);
    }

    /// <summary>
    ///     Re-format literal values for specific types.
    /// </summary>
    /// <param name="value">Object literal value.</param>
    /// <returns>Re-formatted value.</returns>
    private static string? FormatLiteralValue(object value)
    {
        if (value is DateTime dateTimeValue)
            return dateTimeValue.ToUniversalTime()
                .ToString("yyyy-MM-ddTHH:mm:ssZ");

        if (value is double or float)
            return (string)Convert.ChangeType(value,
                typeof(string), CultureInfo.InvariantCulture);

        return value.ToString();
    }

    /// <summary>
    ///     Converts attribute property to RdfTriple.
    /// </summary>
    /// <param name="subject">Rdf triple subject - CIM object.</param>
    /// <param name="attribute"></param>
    /// <returns></returns>
    private object? GetObjectAsAttribute(IModelObject subject,
        ICimMetaProperty attribute)
    {
        object? tripleObject = null;
        if (attribute.PropertyDatatype is ICimMetaDatatype)
        {
            tripleObject = subject.GetAttribute(attribute);
        }
        else if (attribute.PropertyDatatype is { } metaClass)
        {
            if (metaClass.IsEnum)
            {
                var enumObject = subject.GetAttribute(attribute);
                if (enumObject is EnumValueObject enumValueObject)
                    tripleObject = enumValueObject.MetaEnumValue.BaseUri;
                else if (enumObject is Enum typedEnum)
                    tripleObject = metaClass.AllIndividuals
                        .Where(i => i.ShortName == typedEnum.ToString())
                        .Select(i => i.BaseUri)
                        .FirstOrDefault();
            }

            if (metaClass.IsCompound)
            {
                var compoundObject = subject.GetAttribute<IModelObject>(attribute);

                if (compoundObject != null) 
                    tripleObject = ModelObjectToRdfNode(compoundObject);
            }
        }

        return tripleObject;
    }

    /// <summary>
    ///     Converts Assoc1To1 property to RdfTriple.
    /// </summary>
    /// <param name="subject">Rdf triple subject - CIM object.</param>
    /// <param name="assoc1To1">CIM meta property - assoc.</param>
    /// <returns>Rdf property triple.</returns>
    private static Uri? GetObjectAsAssoc1To1(IModelObject subject,
        ICimMetaProperty assoc1To1)
    {
        var assocObj = subject.GetAssoc1To1<IModelObject>(assoc1To1);

        var resultAssocObject = assocObj?.OID.AbsoluteOID;

        return resultAssocObject;
    }

    /// <summary>
    ///     Converts Assoc1ToM property to RdfTriple collection
    /// </summary>
    /// <param name="subject">Rdf triple subject - CIM object.</param>
    /// <param name="assoc1ToM">CIM meta property - assoc.</param>
    /// <returns>Set of Rdf property triples.</returns>
    private static IEnumerable<Uri> GetObjectAsAssoc1ToM(IModelObject subject,
        ICimMetaProperty assoc1ToM)
    {
        return subject.GetAssoc1ToM(assoc1ToM)
            .Select(mo => mo.OID.AbsoluteOID);
    }

    private List<RdfNode> GetObjectAsStatements(IModelObject subject,
        ICimMetaProperty statementsProperty)
    {
        var result = new List<RdfNode>();
        if (subject is IStatementsContainer statementsContainer
            && statementsContainer.Statements.ContainsKey(statementsProperty))
        {
            var statements = statementsContainer.Statements[statementsProperty];

            foreach (var statement in statements)
            {
                var rdfNode = ModelObjectToRdfNode(statement);
                if (rdfNode == null 
                    || rdfNode.TypeIdentifier.AbsoluteUri == RdfDescription 
                    && rdfNode.Triples.Length == 0) continue;

                result.Add(rdfNode);
            }
        }

        return result;
    }

    /// <summary>
    ///     Fill writer namespaces from CimSchema.
    /// </summary>
    private void InitializeWriterNamespaces(ISet<Uri> allowedNamespaces)
    {
        RdfWriter.ClearNamespaces();
        
        foreach (var (prefix, uri) in Schema.Namespaces)
        {
            if (prefix == "base") continue;
            
            if (!allowedNamespaces.Contains(uri)) continue;

            RdfWriter.AddNamespace(prefix, uri);
        }

        RdfWriter.AddNamespace("base", BaseUri);
    }

    #endregion

    #region DeserializerBlock

    private void InitializeRdfReader(StreamReader streamReader)
    {
        if (streamReader == null
            || streamReader.BaseStream.CanSeek == false)
            throw new Exception("No data stream for read!");

        _streamReader = streamReader;
        RdfReader.Load(streamReader);
        RdfReader.AddNamespace("base", BaseUri);
    }

    private void ResetCache()
    {
        _objectsCache = [];
        CachedNamespaces.Clear();
        _waitingReferenceObjects = [];
        _createdAutoClassesCache = [];
        _createdAutoPropertiesCache = [];
    }

    /// <summary>
    ///     Convert RdfNode objects to IModelObject collection.
    /// </summary>
    /// <returns>Collection of IModelObject instances.</returns>
    private List<IModelObject> ReadObjects()
    {
        ResetCache();

        if (_streamReader == null) 
            throw new Exception("Serializer reader has not been initialized!");

        // First step - creating objects.
        foreach (var instanceNode in RdfReader.ReadAll())
        {
            try
            {
                var instance = RdfNodeToModelObject(instanceNode);
                if (instance == null) continue;

                // type redefinition
                if (_objectsCache.TryGetValue(instance.OID, out var cached)
                    && cached.MetaClass.BaseUri.AbsoluteUri == RdfDescription
                    && instance.MetaClass.BaseUri.AbsoluteUri != RdfDescription)
                {
                    _objectsCache.Remove(instance.OID);
                }
                
                _objectsCache.TryAdd(instance.OID, instance);
            }
            catch (Exception ex)
            {
                _log.Error(
                    $"Error raised while {instanceNode.Identifier} " +
                    $"ModelObject constructing: {ex.Message}.",
                    instanceNode);
            }
        }

        // Second step - fill objects properties.
        _streamReader.BaseStream.Position = 0;
        _streamReader.DiscardBufferedData();
        InitializeRdfReader(_streamReader);
        foreach (var instanceNode in RdfReader.ReadAll())
        {
            var instanceOid = OidDescriptorFactory.TryCreate(instanceNode.Identifier);
            if (instanceOid == null || _objectsCache.TryGetValue(instanceOid,
                    out var instance) == false)
                continue;

            foreach (var property in instanceNode.Triples) 
                ReadObjectProperty(instance, property);
            
            instance.Shrink();
        }

        RdfReader.Close();
        _streamReader.Close();

        return [.. _objectsCache.Values];
    }

    /// <summary>
    ///     Build IModelObject instance from RdfNode.
    /// </summary>
    /// <param name="instanceNode">RdfNode CIM object presentation.</param>
    /// <param name="isAuto"></param>
    /// <returns>IModelObject instance or null.</returns>
    private IModelObject? RdfNodeToModelObject(RdfNode instanceNode,
        bool isAuto = false)
    {
        IOIDDescriptor? instanceOid;
        if (isAuto == false)
        {
            instanceOid = OidDescriptorFactory.TryCreate(instanceNode.Identifier);
            if (instanceOid == null) 
                throw new ArgumentException("Failed to create model object oid!");
        }
        else
        {
            instanceOid = new AutoDescriptor();
        }

        var metaClass = Schema.TryGetResource<ICimMetaClass>
            (instanceNode.TypeIdentifier);

        IModelObjectFactory objectFactory = new ModelObjectFactory();
        if (metaClass == null)
        {
            if (Settings.UnknownClassesAllowed)
            {
                var autoClass = GetOrCreateAutoClass(instanceNode.TypeIdentifier);
                autoClass.SetIsCompound(instanceNode.IsAuto);
                objectFactory = new WeakModelObjectFactory();

                metaClass = autoClass;
            }
            else
            {
                _log.Warn(
                    "Skip non-existing schema class "
                    + $"{instanceNode.TypeIdentifier.AbsoluteUri} in "
                    + $"instance {instanceNode.Identifier}",
                    instanceNode);
                
                return null;
            }
        }

        return TypeLib.CreateInstance(objectFactory, instanceOid, metaClass);
    }

    /// <summary>
    ///     Create auto class for Settings.AllowUnknownClasses=true
    /// </summary>
    /// <param name="typeIdentifier">Type URI</param>
    /// <returns>Auto class instance.</returns>
    private CimAutoClass GetOrCreateAutoClass(Uri typeIdentifier)
    {
        if (_createdAutoClassesCache.TryGetValue(typeIdentifier.AbsoluteUri,
                out var cimAutoClass))
            return cimAutoClass;

        var shortName = typeIdentifier.AbsoluteUri;
        if (RdfUtils.TryGetEscapedIdentifier(typeIdentifier, out var sIri)) 
            shortName = sIri;

        var newCimAutoClass = new CimAutoClass(typeIdentifier,
            shortName, string.Empty)
        {
            ParentClass = Schema.ResourceSuperClass
        };

        _createdAutoClassesCache.Add(typeIdentifier.AbsoluteUri,
            newCimAutoClass);

        return newCimAutoClass;
    }

    /// <summary>
    ///     Convert RDF n-triple to IModelObject CIM property.
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
            if (Settings.UnknownPropertiesAllowed
                || instance is WeakModelObject)
            {
                schemaProperty = GetOrCreateAutoProperty(propertyTriple);
            }
            else
            {
                _log.Warn(
                    "Skip non-existing schema property "
                    + $"{propertyTriple.Predicate.AbsoluteUri} in "
                    + $"instance {instance.OID}",
                    instance);

                return;
            }
        }

        if (schemaProperty.PropertyDatatype is null)
        {
            _log.Warn(
                "Skip schema property "
                + $"{propertyTriple.Predicate.AbsoluteUri} in "
                + $"instance {instance.OID} cause of non existing datatype",
                instance);

            return;
        }

        var data = DeserializableDataSelector(propertyTriple.Object);
        if (data == null) return;

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
            case CimMetaPropertyKind.Statements:
            {
                if (instance is IStatementsContainer statementsContainer
                    && data is ICollection<IModelObject> statements)
                    foreach (var statement in statements)
                        statementsContainer
                            .AddToStatements(schemaProperty, statement);

                break;
            }
        }
    }

    private CimAutoProperty GetOrCreateAutoProperty(RdfTriple propertyTriple)
    {
        var propIRI = propertyTriple.Predicate.AbsoluteUri;

        if (_createdAutoPropertiesCache.TryGetValue(propIRI,
                out var cimAutoProperty))
            return cimAutoProperty;

        var shortName = propIRI;
        if (RdfUtils.TryGetEscapedIdentifier(propertyTriple.Predicate,
                out var sIri))
            shortName = sIri.Split('.').Last();

        ICimMetaClass? metaDatatype = null;
        var cimMetaPropertyKind = CimMetaPropertyKind.NonStandard;
        if (propertyTriple.Object is RdfTripleObjectLiteralContainer)
        {
            metaDatatype = Schema.TryGetResource<ICimMetaDatatype>(
                new Uri("http://www.w3.org/2001/XMLSchema#string"));

            cimMetaPropertyKind = CimMetaPropertyKind.Attribute;
        }

        if (propertyTriple.Object is RdfTripleObjectUriContainer)
        {
            metaDatatype = Schema.ResourceSuperClass;

            cimMetaPropertyKind = CimMetaPropertyKind.Assoc1ToM;
        }

        if (propertyTriple.Object is RdfTripleObjectStatementsContainer
            statementsContainer)
        {
            var maybeCompound = statementsContainer.RdfNodesObject
                .FirstOrDefault();
            if (statementsContainer.RdfNodesObject.Count == 1
                && maybeCompound is { IsAuto: true })
            {
                var compoundMetaClass = GetOrCreateAutoClass(
                    maybeCompound.TypeIdentifier);
                compoundMetaClass.SetIsCompound(true);

                metaDatatype = compoundMetaClass;

                cimMetaPropertyKind = CimMetaPropertyKind.Attribute;
            }
            else
            {
                metaDatatype = Schema.ResourceSuperClass;

                cimMetaPropertyKind = CimMetaPropertyKind.Statements;
            }
        }

        var newCimAutoProperty = new CimAutoProperty(
            propertyTriple.Predicate, shortName, string.Empty)
        {
            OwnerClass = Schema.ResourceSuperClass
        };
        newCimAutoProperty.SetPropertyDatatype(metaDatatype);
        newCimAutoProperty.SetPropertyKind(cimMetaPropertyKind);

        _createdAutoPropertiesCache.Add(propIRI, newCimAutoProperty);

        return newCimAutoProperty;
    }

    /// <summary>
    ///     Select expecting IModelObject property data.
    /// </summary>
    /// <returns>Casted data or compound.</returns>
    private object? DeserializableDataSelector(RdfTripleObjectContainerBase data)
    {
        if (data is RdfTripleObjectStatementsContainer statements)
        {
            var compoundObjects = new List<IModelObject>();
            foreach (var statement in statements.RdfNodesObject)
            {
                var compoundObject = MakeCompoundPropertyObject(statement);
                if (compoundObject != null) compoundObjects.Add(compoundObject);
            }

            return compoundObjects;
        }

        if (data is RdfTripleObjectUriContainer uriContainer) 
            return uriContainer.UriObject;

        if (data is RdfTripleObjectLiteralContainer literalContainer) 
            return literalContainer.LiteralObject;

        return null;
    }

    /// <summary>
    ///     Push attribute value to IModelObject.
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
            if (TryConvertValue(data, dataType.PrimitiveType,
                    out var convertedValue))
                endData = convertedValue;
            else
                _log.Error(
                    $"Unable to convert {data} value to " +
                    $"{dataType.PrimitiveType.Name} for {property.ShortName}" +
                    $"of instance {instance.OID}!",
                    instance);
        }
        else if (property.PropertyDatatype is { } dataClass)
        {
            if (dataClass.IsCompound &&
                data is ICollection<IModelObject> { Count: 1 } modelObjects)
            {
                endData = modelObjects.Single();
            }
            else if (dataClass.IsEnum && data is Uri enumValueUri)
            {
                var schemaEnumValue = Schema
                    .TryGetResource<ICimMetaIndividual>(enumValueUri);

                if (schemaEnumValue != null)
                    endData = TypeLib.CreateEnumValueInstance(schemaEnumValue);
                else
                    _log.Error(
                        $"Enum value {enumValueUri} of instance " +
                        $"{instance.OID} does not exist in schema!",
                        instance);
            }
        }

        try
        {
            if (endData != null) instance.SetAttribute(property, endData);
        }
        catch (Exception ex)
        {
            _log.Error("Failed set attribute to instance " +
                       $"{instance.OID}: {ex.Message}", instance);
        }
    }

    private static bool TryConvertValue(object? value, Type type,
        out object? converted)
    {
        converted = null;

        try
        {
            var convertedValue = Convert.ChangeType(value,
                type, CultureInfo.InvariantCulture);

            if (convertedValue != null)
            {
                converted = convertedValue;
                return true;
            }

            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    ///     Push association to IModelObject.
    /// </summary>
    /// <param name="instance">IModelObject instance.</param>
    /// <param name="property">CIM meta property.</param>
    /// <param name="referenceUri">Association object reference by Uri.</param>
    private void SetObjectDataAsAssociation(IModelObject instance,
        ICimMetaProperty property, Uri referenceUri)
    {
        var referenceOid = OidDescriptorFactory.TryCreate(referenceUri);
        if (referenceOid == null) return;

        _objectsCache.TryGetValue(referenceOid, out var referenceInstance);

        if (referenceInstance == null && Settings.IncludeUnresolvedReferences)
            SetAssociationAsUnresolved(instance, property, referenceOid);

        if (referenceInstance != null) 
            SetObjectDataAsAssociation(instance, property, referenceInstance);
    }

    private void SetObjectDataAsAssociation(IModelObject instance,
        ICimMetaProperty property, IModelObject referenceInstance)
    {
        try
        {
            if (property.PropertyKind == CimMetaPropertyKind.Assoc1To1)
                instance.SetAssoc1To1(property,
                    referenceInstance);
            else if (property.PropertyKind == CimMetaPropertyKind.Assoc1ToM)
                instance.AddAssoc1ToM(property,
                    referenceInstance);
        }
        catch (Exception ex)
        {
            _log.Error(
                "Failed to set association between instances " +
                $"{instance.OID} and {referenceInstance.OID}: {ex.Message}",
                instance);
        }
    }

    private void SetAssociationAsUnresolved(IModelObject instance,
        ICimMetaProperty property, IOIDDescriptor unresolvedOID)
    {
        if (_waitingReferenceObjects.TryGetValue(
                (unresolvedOID, property),
                out var existingUnresolved))
        {
            existingUnresolved.WaitingObjects.Add(instance);
            SetObjectDataAsAssociation(instance, property, existingUnresolved);
        }
        else
        {
            var unresolved = new ModelObjectUnresolvedReference(
                unresolvedOID, property);

            unresolved.WaitingObjects.Add(instance);

            _waitingReferenceObjects.TryAdd(
                (unresolvedOID, property), unresolved);

            SetObjectDataAsAssociation(instance, property, unresolved);
        }
    }

    /// <summary>
    ///     Makes auto compound object from RdfNode.
    /// </summary>
    /// <param name="objectRdfNode">Compound property Rdf node.</param>
    /// <returns>Auto IModelObject or null.</returns>
    private IModelObject? MakeCompoundPropertyObject(RdfNode objectRdfNode)
    {
        var compoundPropertyObject = RdfNodeToModelObject(objectRdfNode,
            objectRdfNode.IsAuto);

        if (compoundPropertyObject == null) return null;

        foreach (var property in objectRdfNode.Triples) 
            ReadObjectProperty(compoundPropertyObject, property);

        return compoundPropertyObject;
    }

    #endregion
}
