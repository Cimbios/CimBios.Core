namespace CimBios.CimModel.Context
{
    public interface IModelObject
    {
        public DataFacade ObjectData { get; }
        public string Uuid { get; }
    }

    public class ModelObject : IModelObject
    {
        public ModelObject(DataFacade objectData)
        {
            ObjectData = objectData;
        }

        public string Uuid { get => ObjectData.Uuid; }

        public DataFacade ObjectData { get; }
    }

    public class DataFacade
    {
        public DataFacade(string uuid, string classType)
        {
            _uuid = uuid;
            _classType = classType;

            _attributes = new Dictionary<string, object?>();
            _assocs1to1 = new Dictionary<string, object?>();
            _assocs1toM = new Dictionary<string, List<object>>();
        }

        public string Uuid { get => _uuid; }

        public string ClassType { get => _classType; }

        public string[] Attributes { get => _attributes.Keys.ToArray(); }
        public string[] Assocs1To1 { get => _assocs1to1.Keys.ToArray(); }
        public string[] Assocs1ToM { get => _assocs1toM.Keys.ToArray(); }

        public object? GetAttribute(string attribute)
        {
            return _attributes.ContainsKey(attribute) ? _attributes[attribute] : null;
        }

        public void SetAttribute(string attribute, object? value)
        {
            _attributes[attribute] = value;
        }

        public object? GetAssoc1To1(string assoc)
        {
            return _assocs1to1.ContainsKey(assoc) ? _assocs1to1[assoc] : null;
        }

        public void SetAssoc1To1(string assoc, object obj)
        {
            _assocs1to1[assoc] = obj;
        }

        public void RemoveAssoc1To1(string assoc)
        {
            _assocs1to1.Remove(assoc);
        }

        public object[]? GetAssoc1toM(string assoc)
        {
            return _assocs1toM.ContainsKey(assoc) ? _assocs1toM[assoc].ToArray() : null;
        }

        public void AddAssoc1ToM(string assoc, object obj)
        {
            if (_assocs1toM.ContainsKey(assoc) == false
                || _assocs1toM[assoc] == null)
            {
                _assocs1toM[assoc] = new List<object>();
            }

            _assocs1toM[assoc].Add(obj);
        }

        public void RemoveAssoc1ToM(string assoc, object obj)
        {
            _assocs1toM[assoc].Remove(obj);
        }

        public void RemoveAllAssoc1ToM(string assoc, object obj)
        {
            _assocs1toM[assoc].Clear();
        }

        private string _uuid = string.Empty;
        private string _classType = string.Empty;
        private Dictionary<string, object?> _attributes;
        private Dictionary<string, object?> _assocs1to1;
        private Dictionary<string, List<object>> _assocs1toM;
    }

    internal static class DataFacadeExtension
    {
        internal static void AddAssoc1ToUnk(this DataFacade cimObj, string assoc, object obj)
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
