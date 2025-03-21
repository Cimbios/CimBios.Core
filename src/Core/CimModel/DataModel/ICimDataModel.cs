using CimBios.Core.CimModel.CimDataModel.Utils;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.Headers552;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.Schema;
using CimBios.Utils.ClassTraits;

namespace CimBios.Core.CimModel.CimDataModel;

/// <summary>
/// Definition of CIM model object type.
/// </summary>
public interface ICimDataModel : ICanLog
{
    /// <summary>
    /// Model description.
    /// </summary>
    public Model? ModelDescription { get; }

    /// <summary>
    /// Applied schema to this context serializer.
    /// </summary>
    public ICimSchema Schema { get; }

    /// <summary>
    /// Current context type lib of CIM model objects.
    /// </summary>
    public ICimDatatypeLib TypeLib { get; }

    /// <summary>
    /// Descriptor factory for producing objects.
    /// </summary>
    public IOIDDescriptorFactory OIDDescriptorFactory { get; } 

    /// <summary>
    /// Get all model objects.
    /// </summary>
    /// <returns>IModelObject instance collection.</returns>
    public IEnumerable<IModelObject> GetAllObjects();

    /// <summary>
    /// Get all typed model objects.
    /// </summary>
    /// <returns>IModelObject instance collection.</returns>
    public IEnumerable<T> GetObjects<T>() where T : IModelObject;

    /// <summary>
    /// Get all meta typed model objects.
    /// </summary>
    /// <returns>IModelObject instance collection.</returns>
    public IEnumerable<IModelObject> GetObjects(ICimMetaClass metaClass);

    /// <summary>
    /// Get generalized model object by uuid.
    /// </summary>
    /// <param name="oid"></param>
    /// <returns>IModelObject instance or null.</returns>
    public IModelObject? GetObject(IOIDDescriptor oid);

    /// <summary>
    /// Get typed model object by uuid.
    /// </summary>
    /// <typeparam name="T">IModelObject generalized class.</typeparam>
    /// <param name="oid">Model object string identifier.</param>
    /// <returns>T casted IModelObject instance or null.</returns>
    public T? GetObject<T>(IOIDDescriptor oid) where T : IModelObject;

    /// <summary>
    /// Remove object from model context.
    /// </summary>
    /// <param name="oid">Model object string identifier.</param>
    /// <returns>True if object found and removed.</returns>
    public bool RemoveObject(IOIDDescriptor oid);

    /// <summary>
    /// Remove object from model context.
    /// </summary>
    /// <param name="modelObject">Model object.</param>
    /// <returns>True if object found and removed.</returns>
    public bool RemoveObject(IModelObject modelObject);

    /// <summary>
    /// Remove object from model context.
    /// </summary>
    /// <param name="modelObjects">Enumerable of model objects.</param>
    public void RemoveObjects(IEnumerable<IModelObject> modelObjects);

    /// <summary>
    /// Create IModelObject instance of meta class.
    /// </summary>
    /// <param name="oid">Specific object identifier.</param>
    /// <param name="metaClass">Meta class.</param>
    /// <returns>Create IModelObject instance.</returns>
    public IModelObject CreateObject(IOIDDescriptor oid, ICimMetaClass metaClass);

    /// <summary>
    /// Create IModelObject instance of datatype lib meta class.
    /// </summary>
    /// <typeparam name="T">Datatype lib type.</typeparam>
    /// <param name="oid">Specific object identifier.</param>
    /// <returns>Create T : IModelObject instance.</returns>
    public T CreateObject<T>(IOIDDescriptor oid) where T : class, IModelObject;

    //public IModelObject CreateObject(ICimMetaClass metaClass);
    ///public T CreateObject<T>(string oid) where T: IModelObject;
    //public T CreateObject<T>() where T: IModelObject;

    /// <summary>
    /// Event fires on data model object property changed.
    /// </summary>
    public event CimDataModelObjectPropertyChangedEventHandler? 
        ModelObjectPropertyChanged;

    /// <summary>
    /// Event fires on data model object storage changed - add/remove objects.
    /// </summary>
    public event CimDataModelObjectStorageChangedEventHandler? 
        ModelObjectStorageChanged;
}
