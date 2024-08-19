using System;
using System.ComponentModel;

namespace CimBios.Core.CimModel.CimDatatypeLib
{
    /// <summary>
    /// Structure interface of abstact super model type.
    /// </summary>
    public interface IModelObject
    {
        /// <summary>
        /// Facade for data operations incapsulation.
        /// </summary>
        public IDataFacade ObjectData { get; }

        /// <summary>
        /// Neccesary object identifier.
        /// </summary>
        public string Uuid { get; }
    }

    /// <summary>
    /// Model super type.
    /// </summary>
    public class ModelObject : IModelObject
    {
        public ModelObject(DataFacade objectData)
        {
            ObjectData = objectData;
        }

        public string Uuid { get => ObjectData.Uuid; }

        public IDataFacade ObjectData { get; }
    }

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
        /// Get attribute typed T value. Throws exception if attribute does not exists.
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
        /// Get 1 to 1 assoc object.
        /// </summary>
        /// <param name="assoc">Assoc name in format of 'Domain.Assoc'.</param>
        /// <returns>IModelObject instance or null.</returns>
        public IModelObject? GetAssoc1To1(string assoc);

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
        /// Get 1 to M assoc objects.
        /// </summary>
        /// <param name="assoc">Assoc name in format of 'Domain.Assoc'.</param>
        /// <returns>IModelObject instances array or null.</returns>
        public object[]? GetAssoc1ToM(string assoc);
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
        public DataFacade(string uuid, Uri classType, bool isCompound = false)
        {
            _uuid = uuid;
            _classType = classType;
            _isCompound = isCompound;

            _attributes = new Dictionary<string, object>();
            _assocs1to1 = new Dictionary<string, IModelObject>();
            _assocs1toM = new Dictionary<string, List<IModelObject>>();
        }

        public string Uuid { get => _uuid; }
        public Uri ClassType { get => _classType; }
        public bool IsCompound { get => _isCompound; }
        public string[] Attributes { get => _attributes.Keys.ToArray(); }
        public string[] Assocs1To1 { get => _assocs1to1.Keys.ToArray(); }
        public string[] Assocs1ToM { get => _assocs1toM.Keys.ToArray(); }

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
            _attributes[attribute] = value;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
        }

        public IModelObject? GetAssoc1To1(string assoc)
        {
            return _assocs1to1.ContainsKey(assoc) ? _assocs1to1[assoc] : null;
        }

        public void SetAssoc1To1(string assoc, IModelObject obj)
        {
            _assocs1to1[assoc] = obj;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(assoc));
        }

        public void RemoveAssoc1To1(string assoc)
        {
            _assocs1to1.Remove(assoc);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(assoc));
        }

        public object[]? GetAssoc1ToM(string assoc)
        {
            return _assocs1toM.ContainsKey(assoc) ? _assocs1toM[assoc].ToArray() : null;
        }

        public void AddAssoc1ToM(string assoc, IModelObject obj)
        {
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
            _assocs1toM[assoc].Remove(obj);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(assoc));
        }

        public void RemoveAllAssoc1ToM(string assoc)
        {
            _assocs1toM[assoc].Clear();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(assoc));
        }

        private string _uuid = string.Empty;
        private Uri _classType;
        private bool _isCompound;
        private Dictionary<string, object> _attributes;
        private Dictionary<string, IModelObject> _assocs1to1;
        private Dictionary<string, List<IModelObject>> _assocs1toM;

        public event PropertyChangedEventHandler? PropertyChanged;
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
        public static void AddAssoc1ToUnk(this IDataFacade cimObj, string assoc, IModelObject obj)
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
}
