using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CimBios.Core.CimModel.Validation
{
    public class PropertyMultiplisityValidationRule : ValidationRuleBase
    {
        /// <inheritdoc/>
        public override IEnumerable<ValidationResult> Execute(
            IModelObject modelObject)
        {
            var propertiesRequied = MultiplisityRequired(modelObject);

            var objectsRequired = GetObjectsRequired(
                propertiesRequied, modelObject);

            foreach (var or in objectsRequired)
            {
                yield return ValidationResults(or, modelObject);
            }
        }

        public object?[] GetObjectsRequired(
            IEnumerable<ICimMetaProperty> propertiesRequied, 
            IModelObject modelObject)
        {
            var objectsRequied = new List<object?>();

            foreach (var pq in propertiesRequied)
            {
                switch (pq.PropertyKind)
                {
                    case CimMetaPropertyKind.Attribute:
                        var att = modelObject.GetAttribute(pq);
                        objectsRequied.Add(att);
                        break;
                    case CimMetaPropertyKind.Assoc1To1:
                        var as1To1 = modelObject.GetAssoc1To1<IModelObject>(pq);
                        objectsRequied.Add(as1To1);
                        break;
                    case CimMetaPropertyKind.Assoc1ToM:
                        var as1ToM = modelObject.GetAssoc1ToM(pq);
                        objectsRequied.Add(as1ToM);
                        break;
                }
            }
            return objectsRequied.ToArray();
        }

        private ValidationResult ValidationResults(object? obj,
            IModelObject modelObject)
        {
            //if (obj == null)
            //{
            //    yield return new ValidationResult()
            //    { 
            //        Message = "",
            //        ResultType = ValidationResultKind.Fail,
            //        ModelObject = modelObject
            //    };
            //}
            return obj == null 
                ? new ValidationResult()
                {
                    Message = "Ошибка",
                    ResultType = ValidationResultKind.Fail,
                    ModelObject = modelObject
                } 
                : new ValidationResult()
                {
                    Message = "Всё норм",
                    ResultType = ValidationResultKind.Pass,
                    ModelObject = modelObject
                };
        }

        private IEnumerable<ICimMetaProperty> MultiplisityRequired(
            IModelObject modelObject)
        {
            var properties = modelObject.MetaClass.AllProperties;

            foreach (var property in properties)
            {
                if(property.IsValueRequired)
                {
                    yield return property;
                }
            }
        }
    }
}
