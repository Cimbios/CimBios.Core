using System.ComponentModel;

namespace CimBios.Core.CimModel.CimDatatypeLib;

public interface IDataFacade : INotifyPropertyChanged
{
    /// <summary>
    /// Neccesary object identifier.
    /// </summary>
    public string Uuid { get; }

    /// <summary>
    /// Class type name.
    /// </summary>
    public Uri ClassType { get; }

    /// <summary>
    /// Unidentified object status
    /// </summary>
    public bool IsAuto { get; }

    /// <summary>
    /// Compound predicate status
    /// </summary>
    public bool IsCompound { get; }

    /// <summary>
    /// List of all non-empty attributes.
    /// </summary>
    public string[] Attributes { get; }

    /// <summary>
    /// List of all non-empty 1 to 1 multiplicity assocs.
    /// </summary>
    public string[] Assocs1To1 { get; }

    /// <summary>
    /// List of all non-empty 1 to N multiplicity assocs.
    /// </summary>
    public string[] Assocs1ToM { get; }

    /// <summary>
    /// Check is property exists method.
    /// </summary>
    /// <param name="property">String property name.</param>
    /// <returns>True if exists like attribute or assoc.</returns>
    public bool HasProperty(string property);

    /// <summary>
    /// Get attribute value. Throws exception if property does not exists.
    /// </summary>
    /// <param name="property"></param>
    /// <param name="type"></param>
    /// <returns>Value.</returns>
    public object GetAttribute(string property);

    /// <summary>
    /// Get attribute typed value. Throws exception if property does not exists.
    /// </summary>
    /// <param name="property"></param>
    /// <param name="type"></param>
    /// <returns>Value.</returns>
    public object GetAttribute(string property, Type type);

    /// <summary>
    /// Get attribute typed T value. Throws exception if property does not exists.
    /// </summary>
    /// <param name="attribute">Attribute name in format of 'Domain.Attribute'.</param>
    /// <returns>Typed value.</returns>
    public T GetAttribute<T>(string attribute) where T : class;

    /// <summary>
    /// Set attribute typed T value.
    /// </summary>
    /// <param name="attribute">Attribute name in format of 'Domain.Attribute'.</param>
    /// <param name="value">Typed value.</param>
    public void SetAttribute<T>(string attribute, T value) where T : class;

    /// <summary>
    /// Get 1 to 1 assoc object. Throws exception if property does not exists.
    /// </summary>
    /// <param name="assoc">Assoc name in format of 'Domain.Assoc'.</param>
    /// <returns>IModelObject instance.</returns>
    public IModelObject GetAssoc1To1(string assoc);

    /// <summary>
    /// Set 1 to 1 assoc object.
    /// </summary>
    /// <param name="assoc">Assoc name in format of 'Domain.Assoc'.</param>
    /// <param name="obj">IModelObject instance.</param>
    public void SetAssoc1To1(string assoc, IModelObject obj);

    /// <summary>
    /// Remove 1 to 1 assoc beetween objects.
    /// </summary>
    /// <param name="assoc">Assoc name in format of 'Domain.Assoc'.</param>
    public void RemoveAssoc1To1(string assoc);

    /// <summary>
    /// Get 1 to M assoc objects. Throws exception if property does not exists.
    /// </summary>
    /// <param name="assoc">Assoc name in format of 'Domain.Assoc'.</param>
    /// <returns>IModelObject instances array.</returns>
    public object[] GetAssoc1ToM(string assoc);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="assoc"></param>
    /// <param name="obj"></param>
    public void AddAssoc1ToM(string assoc, IModelObject obj);

    /// <summary>
    /// Remove 1 to M assoc beetween objects.
    /// </summary>
    /// <param name="assoc">Assoc name in format of 'Domain.Assoc'.</param>
    /// <param name="obj">IModelObject associated instance.</param>
    public void RemoveAssoc1ToM(string assoc, IModelObject obj);

    /// <summary>
    /// Remove all 1 to M assocs beetween objects.
    /// </summary>
    /// <param name="assoc">Assoc name in format of 'Domain.Assoc'.</param>
    public void RemoveAllAssoc1ToM(string assoc);
}

/// <summary>
/// Facade for data operations incapsulation.
/// </summary>
public class DataFacade : IDataFacade
{
    public string Uuid { get => _uuid; }
    public Uri ClassType { get => _classType; }
    public bool IsAuto { get => _isAuto; }
    public bool IsCompound { get => _isCompound; }
    public string[] Attributes { get => _attributes.Keys.ToArray(); }
    public string[] Assocs1To1 { get => _assocs1to1.Keys.ToArray(); }
    public string[] Assocs1ToM { get => _assocs1toM.Keys.ToArray(); }

    public DataFacade(string uuid, Uri classType,
        bool isAuto = false, bool isCompound = false)
    {
        _uuid = uuid;
        _classType = classType;
        _isAuto = isAuto;
        _isCompound = isCompound;

        _attributes = new Dictionary<string, object>();
        _assocs1to1 = new Dictionary<string, IModelObject>();
        _assocs1toM = new Dictionary<string, List<IModelObject>>();
    }

    public bool HasProperty(string property)
    {
        return _attributes.ContainsKey(property)
            || _assocs1to1.ContainsKey(property)
            || _assocs1toM.ContainsKey(property);
    }

    public object GetAttribute(string attribute)
    {
        if (_attributes.TryGetValue(attribute, out var value))
        {
            return value;
        }

        throw new ArgumentException($"Attribute {attribute} does not exists!");
    }

    public object GetAttribute(string attribute, Type attributeType)
    {
        if (_attributes.TryGetValue(attribute, out var value)
            && value.GetType() == attributeType)
        {
            return value;
        }

        throw new ArgumentException($"Attribute {attribute} does not exists!");
    }

    public T GetAttribute<T>(string attribute) where T : class
    {
        if (_attributes.TryGetValue(attribute, out var value)
            && value is T typedValue)
        {
            return typedValue;
        }

        throw new ArgumentException($"Attribute {attribute} does not exists!");
    }

    public void SetAttribute<T>(string attribute, T value) where T : class
    {
        if (CanChangeProperty(attribute) == false)
        {
            return;
        }

        _attributes[attribute] = value;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
    }

    public IModelObject GetAssoc1To1(string assoc)
    {
        if (_assocs1to1.TryGetValue(assoc, out var assocRef))
        {
            return assocRef;
        }

        throw new ArgumentException($"Association {assoc} does not exists!");
    }

    public void SetAssoc1To1(string assoc, IModelObject obj)
    {
        if (CanChangeProperty(assoc) == false)
        {
            return;
        }

        _assocs1to1[assoc] = obj;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(assoc));
    }

    public void RemoveAssoc1To1(string assoc)
    {
        if (CanChangeProperty(assoc) == false)
        {
            return;
        }
        
        _assocs1to1.Remove(assoc);

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(assoc));
    }

    public object[] GetAssoc1ToM(string assoc)
    {
        if (_assocs1toM.TryGetValue(assoc, out var assocRef))
        {
            return assocRef.ToArray();
        }

        throw new ArgumentException($"Association {assoc} does not exists!");
    }

    public void AddAssoc1ToM(string assoc, IModelObject obj)
    {
        if (CanChangeProperty(assoc) == false)
        {
            return;
        }

        if (_assocs1toM.ContainsKey(assoc) == false
            || _assocs1toM[assoc] == null)
        {
            _assocs1toM[assoc] = new List<IModelObject>();
        }

        _assocs1toM[assoc].Add(obj);

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(assoc));
    }

    public void RemoveAssoc1ToM(string assoc, IModelObject obj)
    {
        if (CanChangeProperty(assoc) == false)
        {
            return;
        }

        if (_assocs1toM.ContainsKey(assoc) == false)
        {
            throw new ArgumentException($"Assoc {assoc} does not exists!");
        }

        _assocs1toM[assoc].Remove(obj);

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(assoc));
    }

    public void RemoveAllAssoc1ToM(string assoc)
    {
        if (CanChangeProperty(assoc) == false)
        {
            return;
        }

        if (_assocs1toM.ContainsKey(assoc) == false)
        {
            throw new ArgumentException($"Assoc {assoc} does not exists!");
        }

        _assocs1toM[assoc].Clear();

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(assoc));
    }

    private bool CanChangeProperty(string propertyName)
    {
        if (PropertyChanging != null)
        {
            var arg = new CanCancelPropertyChangingEventArgs(propertyName, false);

            PropertyChanging.Invoke(this, arg);
            
            if (arg.Cancel == true)
            {
                return false;
            }
        }      

        return true;
    }

    private string _uuid = string.Empty;
    private Uri _classType;
    private bool _isAuto;
    private bool _isCompound;
    private Dictionary<string, object> _attributes;
    private Dictionary<string, IModelObject> _assocs1to1;
    private Dictionary<string, List<IModelObject>> _assocs1toM;

    public event PropertyChangedEventHandler? PropertyChanged;

    public delegate void CanCancelPropertyChangingEventHandler(object? sender, 
        CanCancelPropertyChangingEventArgs e);
    public event CanCancelPropertyChangingEventHandler? PropertyChanging;
}

/// <summary>
/// Extension methods for DataFacade class.
/// </summary>
public static class DataFacadeExtension
{
    /// <summary>
    /// Add object to unknown multiplicity assoc.
    /// 1To1 for the first object, 1toN for more than one objects.
    /// </summary>
    public static void AddAssoc1ToUnk(this IDataFacade cimObj,
        string assoc, IModelObject obj)
    {
        var assoc1To1Obj = cimObj.GetAssoc1To1(assoc);
        if (assoc1To1Obj == null)
        {
            cimObj.SetAssoc1To1(assoc, obj);
        }
        else
        {
            cimObj.AddAssoc1ToM(assoc, assoc1To1Obj);
            cimObj.RemoveAssoc1To1(assoc);

            cimObj.AddAssoc1ToM(assoc, obj);
        }
    }
}

/// <summary>
/// Can cancel functionality for PropertyChangingEventArgs.
/// </summary>
public class CanCancelPropertyChangingEventArgs : PropertyChangingEventArgs
{
    public CanCancelPropertyChangingEventArgs(string? propertyName, bool cancel)
        : base(propertyName)
    {
        Cancel = cancel;
    }

    /// <summary>
    /// Cancel property changing flag.
    /// </summary>
    public virtual bool Cancel { get; set; }
}
