using System.ComponentModel;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.CimDatatypeLib.EventUtils;

namespace CimBios.Core.CimModel.CimDatatypeLib;

/// <summary>
/// CIM object abstaction view. 
/// Provides read and modification logic with data validation.
/// </summary>
public interface IModelObject : INotifyPropertyChanged, IReadOnlyModelObject
{
    /// <summary>
    /// Set attribute typed T value.
    /// </summary>
    /// <param name="metaProperty">Schema meta property instance.</param>
    /// <param name="value">Typed value.</param>
    public void SetAttribute<T>(ICimMetaProperty metaProperty, T? value);

    /// <summary>
    /// Set attribute typed T value.
    /// </summary>
    /// <param name="attributeName">Attribute name in format of '(Domain.)Attribute'.</param>
    /// <param name="value">Typed value.</param>
    public void SetAttribute<T>(string attributeName, T? value);

    /// <summary>
    /// Set 1 to 1 assoc object or clear assoc if obj is null.
    /// </summary>
    /// <param name="metaProperty">Schema meta property instance.</param>
    /// <param name="obj">IModelObject instance.</param>
    public void SetAssoc1To1(ICimMetaProperty metaProperty, IModelObject? obj);

    /// <summary>
    /// Set 1 to 1 assoc object.
    /// </summary>
    /// <param name="assocName">Assoc name in format of 'Domain.Assoc'.</param>
    /// <param name="obj">IModelObject instance.</param>
    public void SetAssoc1To1(string assocName, IModelObject? obj);

    /// <summary>
    /// Add 1 to M assoc beetween objects.
    /// </summary>
    /// <param name="metaProperty">Schema meta property instance.</param>
    /// <param name="obj">IModelObject associated instance.</param>
    public void AddAssoc1ToM(ICimMetaProperty metaProperty, IModelObject obj);

    /// <summary>
    /// Add 1 to M assoc beetween objects.
    /// </summary>
    /// <param name="assocName">Assoc name in format of '(Domain.)Assoc'.</param>
    /// <param name="obj">IModelObject associated instance.</param>
    public void AddAssoc1ToM(string assocName, IModelObject obj);

    /// <summary>
    /// Remove 1 to M assoc beetween objects.
    /// </summary>
    /// <param name="metaProperty">Schema meta property instance.</param>
    /// <param name="obj">IModelObject associated instance.</param>
    public void RemoveAssoc1ToM(ICimMetaProperty metaProperty, IModelObject obj);

    /// <summary>
    /// Remove 1 to M assoc beetween objects.
    /// </summary>
    /// <param name="assocName">Assoc name in format of '(Domain.)Assoc'.</param>
    /// <param name="obj">IModelObject associated instance.</param>
    public void RemoveAssoc1ToM(string assocName, IModelObject obj);

    /// <summary>
    /// Remove all 1 to M assocs beetween objects.
    /// </summary>
    /// <param name="metaProperty">Schema meta property instance.</param>
    public void RemoveAllAssocs1ToM(ICimMetaProperty metaProperty);

    /// <summary>
    /// Remove all 1 to M assocs beetween objects.
    /// </summary>
    /// <param name="assocName">Assoc name in format of '(Domain.)Assoc'.</param>
    public void RemoveAllAssocs1ToM(string assocName);

    /// <summary>
    /// Event fires before changing property value.
    /// </summary>
    public event CanCancelPropertyChangingEventHandler? PropertyChanging;

    /// <summary>
    /// Get read only wrapper for model object.
    /// </summary>
    /// <returns>IModelObject instances array.</returns>
    public IReadOnlyModelObject AsReadOnly();
}

/// <summary>
/// Model object factory provides activation method.
/// </summary>
public interface IModelObjectFactory
{
    /// <summary>
    /// Factory producing type info.
    /// </summary>
    public System.Type ProduceType { get; }

    /// <summary>
    /// Create IModelObject instance.
    /// </summary>
    /// <param name="uuid">Object uuid.</param>
    /// <param name="metaClass">Schema meta class.</param>
    /// <param name="isAuto">Is creating object auto.</param>
    /// <returns>IModelObject instance.</returns>
    public IModelObject Create(string uuid, ICimMetaClass metaClass, bool isAuto);
}
