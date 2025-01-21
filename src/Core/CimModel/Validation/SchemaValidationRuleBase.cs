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
        public ICimSchema Schema { get; }

        /// <inheritdoc/>
        public abstract IEnumerable<ValidationResult> Execute(IModelObject modelObject);

        protected SchemaValidationRuleBase(ICimSchema schema)
        {
            Schema = schema;
        }
    }
}
