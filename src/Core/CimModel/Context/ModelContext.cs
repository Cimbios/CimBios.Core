using System.ComponentModel;
using System.Data;
using System.Xml.Linq;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.RdfXmlIOLib;

namespace CimBios.Core.CimModel.Context
{
    /// <summary>
    /// Instance of CIM model in Rdf/XML format.
    /// Supports input and output operations for CIM objects.
    /// </summary>
    public class ModelContext
    {
        public ModelContext()
        {
            _Objects = new Dictionary<string, IModelObject>();

            _PrivateObjects = new Dictionary<string, IModelObject>();

            _WaitForReferenceDict = new Dictionary
                <string, List<(string, string)>>();
        }

        public ModelContext(TextReader textReader, 
            XNamespace baseNamespace) : this()
        {
            Load(textReader, baseNamespace);
        }

        public void Load(TextReader textReader, XNamespace baseNamespace)
        {
            _Objects.Clear();

            _Reader.Namespaces.Add("base", baseNamespace);
            _Reader.Load(textReader);
            ReadObjects();
        }

        public void Load(string path)
        {
            Load(new StreamReader(path), new System.Uri(path).AbsoluteUri);
        }

        public IEnumerable<IModelObject> GetAllObjects()
        {
            return _Objects.ToList().Select(kvp => kvp.Value);
        }

        public IModelObject? GetObject(string uuid)
        {
            if (_Objects.TryGetValue(uuid, out var instance))
            {
                instance.ObjectData.PropertyChanged += Notify_PropertyChanged;

                return instance;
            }
            else
            {
                return null;
            }
        }

        private void Notify_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
        //    throw new NotImplementedException();
        }

        private void ReadObjects()
        {
            _WaitForReferenceDict.Clear();

            foreach (RdfNode instanceNode in _Reader.ReadAll())
            {
                var instance = CreateInstance(instanceNode);
                if (instance == null)
                {
                    continue;
                }

                if (instance is IFullModel)
                {
                    Description = instance as IFullModel;
                    continue;
                }

                if (instanceNode.IsAuto == true)
                {
                    _PrivateObjects.TryAdd(instance.Uuid, instance);
                }
                else if (instanceNode.IsAuto == false)
                {
                    _Objects.TryAdd(instance.Uuid, instance);
                }
            }

            ResolveWaitingReferenceObjects();
        }

        private void ResolveWaitingReferenceObjects()
        {
            foreach (var uuid in _WaitForReferenceDict.Keys)
            {
                foreach (var kvp in _WaitForReferenceDict[uuid])
                {
                    var instanceUuid = kvp.Item1;
                    var waitingProperty = kvp.Item2;
                    var waitingInstance = GetObject(instanceUuid);
                    if (waitingInstance != null)
                    {
                        if (_PrivateObjects.ContainsKey(uuid) == true)
                        {
                            waitingInstance.ObjectData
                                .SetAttribute(waitingProperty, _PrivateObjects[uuid]);
                        }
                        else if (_Objects.ContainsKey(uuid) == true)
                        {
                            waitingInstance.ObjectData
                                .AddAssoc1ToUnk(waitingProperty, _Objects[uuid]);
                        }
                    }
                    else
                    {
                        if (_PrivateObjects.TryGetValue(uuid,
                            out var waitingPrivateInstance))
                        {
                            waitingPrivateInstance.ObjectData
                                .SetAttribute(waitingProperty, _PrivateObjects[uuid]);
                        }
                    }
                }

                _WaitForReferenceDict.Remove(uuid);
            }
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

            DataFacade objectData = new DataFacade(
                instanceUuid,
                instanceNode.TypeIdentifier,
                IsCompound);

            IModelObject? instanceObject = null;

            if (instanceNode.TypeIdentifier.Fragment == "#FullModel")
            {
                instanceObject = new FullModel(objectData);
            }
            else if (TypesLib != null && TypesLib.RegisteredTypes
                .TryGetValue(instanceNode.TypeIdentifier, out var type))
            {
                instanceObject = Activator.CreateInstance(type, objectData) as IModelObject;
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

                    var referenceInstance = GetObject(referenceUuid);
                    if (referenceInstance == null)
                    {
                        if (_WaitForReferenceDict.ContainsKey(referenceUuid))
                        {
                            _WaitForReferenceDict[referenceUuid]
                                .Add((instance.Uuid, predicate));
                        }
                        else
                        {
                            _WaitForReferenceDict.Add(referenceUuid,
                                new List<(string, string)>()
                                    { (instance.Uuid, predicate) });
                        }
                    }
                    else
                    {
                        instance.ObjectData.AddAssoc1ToUnk(predicate,
                            referenceInstance);
                    }
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
                    && RdfXmlReaderUtils.RdfUriEquals(uri, subObject.Identifier)).Single();

                string predicate = triple.Predicate
                    .Fragment.Replace("#", "");

                instance.ObjectData.SetAttribute(
                       predicate,
                       subObjectInstance);
            }

            return instance;
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

        public IFullModel? Description { get; set; }

        public ContextSettings Settings { get; set; } = new ContextSettings();
        public ICimSchema? Schema { get; set; }
        public IDatatypeLib? TypesLib { get; set; }

        private Dictionary<string, IModelObject> _Objects { get; }
        private Dictionary<string, IModelObject> _PrivateObjects { get; }
        private Dictionary<string, List<(string, string)>> _WaitForReferenceDict { get; }

        private RdfXmlReader _Reader { get; } = new RdfXmlReader();
    }

    public class ContextSettings
    {
        public bool AllowUnkownClassTypes { get; set; } = true;
        public bool AllowUnkownClassProperties { get; set; } = true;
        public bool AllowUriPathMismatches { get; set; } = true;
    }

    public interface IFullModel : IModelObject
    {
        public string Created { get; set; }
        public string Version { get; set; }
    }

    public class FullModel : IFullModel
    {
        public string Uuid { get => ObjectData.Uuid; }
        public string Created 
        { 
            get => ObjectData.GetAttribute<string>("Model.created"); 
            set => ObjectData.SetAttribute("Model.created", value); 
        }
        public string Version
        { 
            get => ObjectData.GetAttribute<string>("Model.version"); 
            set => ObjectData.SetAttribute("Model.version", value);
        }

        public IDataFacade ObjectData { get; }

        public FullModel(DataFacade objectData)
        {
            ObjectData = objectData;
        }
    }
}
