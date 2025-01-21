using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CimBios.Core.CimModel.CimDatatypeLib;

namespace CimBios.Core.CimModel.Validation
{
    public interface IValidationRule
    {
        /// <summary>
        /// Метод для получения списка результатов проверки объекта CIM
        /// </summary>
        /// <returns>Список результатов проверки объекта CIM</returns>
        IEnumerable<ValidationResult> Execute(IModelObject modelObject);
    }
}
