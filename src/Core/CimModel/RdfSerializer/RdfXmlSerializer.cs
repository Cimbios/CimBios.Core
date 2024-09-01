using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.DataProvider;
using CimBios.Core.RdfXmlIOLib;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace CimBios.Core.CimModel.RdfSerializer;

public class RdfXmlSerializer : RdfSerializerBase
{
    public RdfXmlSerializer(RdfXmlFileDataProvider provider) 
        : base(provider)
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
            if (modelObject.ObjectData.GetAssoc1To1(property).ObjectData.IsCompound) // Ожидается, что если это isCompound
            {
                var compound = modelObject.ObjectData.GetAssoc1To1(property);
                var compoundNode = ProcessObject(compound); // Рекурсия
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

    private IEnumerable<IModelObject> ReadObjects()
    {
        _objectsCache.Clear();
        _waitingReferenceObjectUuids.Clear();

        if (_reader == null)
        {
            throw new Exception("Reader was not initialized!");
        }

        foreach (RdfNode instanceNode in _reader.ReadAll())
        {
            var instance = CreateInstance(instanceNode, false);
            if (instance == null)
            {
                continue;
            }

            _objectsCache.TryAdd(instance.Uuid, instance);
        }

        ResolveWaitingReferenceObjects();

        return _objectsCache.Values.ToList();
    }

    private IModelObject? CreateInstance(RdfNode instanceNode,
        bool IsCompound = false)
    {
        string instanceUuid = string.Empty;
        if (TryGetEscapedIdentifier(instanceNode.Identifier,
            out instanceUuid) == false)
        {
            return null;
        }

        if (Settings.AllowUnkownClassProperties == false
            && Schema != null
            && Schema.TryGetDescription<ICimMetaClass>
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
        
        if (TypeLib != null && TypeLib.RegisteredTypes
            .TryGetValue(instanceNode.TypeIdentifier, out var type))
        {
            instanceObject = Activator.CreateInstance(type, objectData) 
                as IModelObject;
        }
        else
        {
            instanceObject = new ModelObject(objectData);
        }

        if (instanceObject != null)
        {
            instanceObject = FillObjectData(instanceObject, instanceNode);
        }

        return instanceObject;
    }

    private IModelObject FillObjectData(IModelObject instance,
        RdfNode instanceNode)
    {
        foreach (var property in instanceNode.Triples)
        {
            string predicate = property.Predicate
                .Fragment.Replace("#", "");

            if (property.Object is string objectString)
            {
                instance.ObjectData.SetAttribute(
                    predicate,
                    objectString);
            }
            else if (property.Object is Uri referenceUri)
            {
                string referenceUuid = string.Empty;
                if (TryGetEscapedIdentifier(referenceUri,
                    out referenceUuid) == false)
                {
                    continue;
                }

                IModelObject referenceInstance;
                if (_objectsCache.ContainsKey(referenceUuid))
                {
                    referenceInstance = _objectsCache[referenceUuid];
                }
                else
                {
                    referenceInstance = 
                        new ModelObjectUnresolvedReference(
                            new DataFacade(referenceUuid, 
                                property.Predicate)
                        );

                    _waitingReferenceObjectUuids.Add(instance.Uuid);
                }

                instance.ObjectData.AddAssoc1ToUnk(predicate,
                    referenceInstance);
            }
        }

        foreach (var subObject in instanceNode.Children)
        {
            var subObjectInstance = CreateInstance(subObject, true);
            if (subObjectInstance == null)
            {
                continue;
            }

            var triple = instanceNode.Triples.Where(t => t.Object is Uri uri
                && RdfXmlReaderUtils.RdfUriEquals(uri, subObject.Identifier))
                    .Single();

            string predicate = triple.Predicate
                .Fragment.Replace("#", "");

            instance.ObjectData.SetAttribute(
                    predicate,
                    subObjectInstance);
        }

        return instance;
    }

    private void ResolveWaitingReferenceObjects()
    {
        foreach (var instanceUuid in _waitingReferenceObjectUuids)
        {
            var instance = _objectsCache[instanceUuid];

            foreach (var assoc1To1 in instance.ObjectData.Assocs1To1)
            {
                if (instance.ObjectData.GetAssoc1To1(assoc1To1) 
                        is ModelObjectUnresolvedReference unresolved
                    && _objectsCache.TryGetValue(unresolved.Uuid, 
                        out var referenceInstance))
                {
                    instance.ObjectData.SetAssoc1To1(assoc1To1, 
                        referenceInstance);
                }
            }

            foreach (var assoc1ToM in instance.ObjectData.Assocs1ToM)
            {
                if (instance.ObjectData.GetAssoc1To1(assoc1ToM)
                        is ModelObjectUnresolvedReference unresolved
                    && _objectsCache.TryGetValue(unresolved.Uuid,
                        out var referenceInstance))
                {
                    instance.ObjectData.RemoveAssoc1ToM(assoc1ToM, unresolved);

                    instance.ObjectData.AddAssoc1ToM(assoc1ToM,
                        referenceInstance);
                }
            }

            _waitingReferenceObjectUuids.Remove(instanceUuid);
        }
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
