using CimBios.Core.CimModel.CimDatatypeLib;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using CimBios.Core.CimModel.Schema;
using System.Threading.Tasks;

namespace CimBios.Core.CimModel.Validation
{
    public abstract class ValidationRuleBase : IValidationRule //PropertyMultiplisityValidationRule
    {
        /// <inheritdoc/>
        public abstract IEnumerable<ValidationResult> Execute(
            IModelObject modelObject);

        /// <summary>
        /// Конструктор класса ValidationRuleBase
        /// </summary>
        protected ValidationRuleBase()
        {

        }
    }
}
