using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CimBios.Core.CimModel.CimDatatypeLib;

namespace CimBios.Core.CimModel.Validation
{
    public class ValidationResult
    {
        /// <summary>
        /// Тип результата валидации
        /// </summary>
        public ValidationResultType ResultType;

        /// <summary>
        /// Сообщение после проверки
        /// </summary>
        public string? Message;

        /// <summary>
        /// Объект CIM
        /// </summary>
        public IModelObject? ModelObject;
    }

    public enum ValidationResultType
    {
        pass,
        fail,
        warning
    }
}
