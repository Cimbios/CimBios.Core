using System.ComponentModel;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.CimDatatypeLib;

/// <summary>
/// CIM object abstaction view. 
/// Provides read and modification logic with data validation.
/// </summary>
public interface IModelObject : INotifyPropertyChanged
{
    /// <summary>
    /// Neccesary object identifier.
    /// </summary>
    public string Uuid { get; }

    /// <summary>
    /// Schema meta class.
    /// </summary>
    public ICimMetaClass MetaClass { get; }

    /// <summary>
    /// Unidentified object status
    /// </summary>
    public bool IsAuto { get; }

    /// <summary>
    /// Check is property exists method.
    /// </summary>
    /// <param name="propertyName">String property name.</param>
    /// <returns>True if exists like attribute or assoc.</returns>
    public bool HasProperty(string propertyName);

    /// <summary>
    /// Get attribute value by meta property instance. 
    /// Throws exception if property does not exists.
    /// </summary>
    /// <param name="metaProperty">Schema meta property instance.</param>
    /// <returns>Value.</returns>
    public object? GetAttribute(ICimMetaProperty metaProperty);

    /// <summary>
    /// Get attribute value by property name. 
    /// Throws exception if property does not exists.
    /// </summary>
    /// <param name="attributeName">Attribute name in format of '(Domain.)Attribute'.</param>
    /// <returns>Value.</returns>
    public object? GetAttribute(string attributeName);

    /// <summary>
    /// Get attribute typed T value. Throws exception if property does not exists.
    /// </summary>
    /// <param name="metaProperty">Schema meta property instance.</param>
    /// <returns>Typed value.</returns>
    public T? GetAttribute<T>(ICimMetaProperty metaProperty);

    /// <summary>
    /// Get attribute typed T value. Throws exception if property does not exists.
    /// </summary>
    /// <param name="attributeName">Attribute name in format of '(Domain.)Attribute'.</param>
    /// <returns>Typed value.</returns>
    public T? GetAttribute<T>(string attributeName);

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
    /// Get 1 to 1 assoc object. Throws exception if property does not exists.
    /// </summary>
    /// <param name="metaProperty">Schema meta property instance.</param>
    /// <returns>IModelObject instance.</returns>
    public T? GetAssoc1To1<T>(ICimMetaProperty metaProperty) where T: IModelObject;

    /// <summary>
    /// Get 1 to 1 assoc object. Throws exception if property does not exists.
    /// </summary>
    /// <param name="assocName">Assoc name in format of '(Domain.)Assoc'.</param>
    /// <returns>IModelObject instance.</returns>
    public T? GetAssoc1To1<T>(string assocName) where T: IModelObject;

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
    /// Get 1 to M assoc objects. Throws exception if property does not exists.
    /// </summary>
    /// <param name="metaProperty">Schema meta property instance.</param>
    /// <returns>IModelObject instances array.</returns>
    public IModelObject[] GetAssoc1ToM(ICimMetaProperty metaProperty);

    /// <summary>
    /// Get 1 to M assoc objects. Throws exception if property does not exists.
    /// </summary>
    /// <param name="assocName">Assoc name in format of 'Domain.Assoc'.</param>
    /// <returns>IModelObject instances array.</returns>
    public IModelObject[] GetAssoc1ToM(string assocName);

    /// <summary>
    /// Get 1 to M assoc objects. Throws exception if property does not exists.
    /// </summary>
    /// <param name="metaProperty">Schema meta property instance.</param>
    /// <returns>IModelObject instances array.</returns>
    public T[] GetAssoc1ToM<T>(ICimMetaProperty metaProperty) 
        where T: IModelObject;

    /// <summary>
    /// Get 1 to M assoc objects. Throws exception if property does not exists.
    /// </summary>
    /// <param name="assocName">Assoc name in format of 'Domain.Assoc'.</param>
    /// <returns>IModelObject instances array.</returns>
    public T[] GetAssoc1ToM<T>(string assocName)
        where T: IModelObject;

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