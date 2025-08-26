using CimBios.Core.CimModel.CimDatatypeLib;
using System.Reflection;

namespace CimBios.Core.CimModel.Validation.Script;
public static class AssemblyInfo
{
    public static Assembly[] References { get; } =
    [
        typeof(object).Assembly,
        typeof(Uri).Assembly,
        typeof(Enumerable).Assembly,
        typeof(IModelObject).Assembly,
        typeof(ModelObject).Assembly,
        typeof(ValidationRuleBase).Assembly
    ];

    public static string[] Usings { get; } =
    [
        "System",
        "System.Collections.Generic",
        "System.Linq",
        "CimBios.Core.CimModel.CimDatatypeLib",
        "CimBios.Core.CimModel.Validation",
        "CimBios.Core.CimModel.CimDatatypeLib.CIM17Types"
    ];
}
