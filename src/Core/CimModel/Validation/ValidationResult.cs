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
        public ValidationResultType ResultType;

        public string? Message;

        public IModelObject? ModelObject;
    }

    public enum ValidationResultType
    {
        pass,
        fail,
        warning
    }
}
