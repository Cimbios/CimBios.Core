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
    public abstract class SchemaValidationRuleBase : IValidationRule
    {
        /// <summary>
        /// Каноническая схема CIM
        /// </summary>
        public ICimSchema Schema { get; }

        /// <inheritdoc/>
        public abstract IEnumerable<ValidationResult> Execute(
            IModelObject modelObject);

        /// <summary>
        /// Конструктор SchemaValidationRuleBase
        /// </summary>
        /// <param name="schema">Каноническая схема CIM</param>
        protected SchemaValidationRuleBase(ICimSchema schema)
        {
            Schema = schema;
        }
    }
}
