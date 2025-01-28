using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;
using CimBios.Utils.ClassTraits;

namespace CimBios.Core.CimModel.ObjectModel;

/// <summary>
/// Definition of CIM model object type.
/// </summary>
public interface IObjectModel : ICanLog
{
    /// <summary>
    /// Model description.
    /// </summary>
    public FullModel? Description { get; }

    /// <summary>
    /// Applied schema to this context serializer.
    /// </summary>
    public ICimSchema? Schema { get; }

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
    /// <param name="uuid"></param>
    /// <returns>IModelObject instance or null.</returns>
    public IModelObject? GetObject(string uuid);

    /// <summary>
    /// Get typed model object by uuid.
    /// </summary>
    /// <typeparam name="T">IModelObject generalized class.</typeparam>
    /// <param name="uuid">Model object string identifier.</param>
    /// <returns>T casted IModelObject instance or null.</returns>
    public T? GetObject<T>(string uuid) where T : IModelObject;

    /// <summary>
    /// Remove object from model context.
    /// </summary>
    /// <param name="uuid">Model object string identifier.</param>
    /// <returns>True if object found and removed.</returns>
    public bool RemoveObject(string uuid);

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
}
