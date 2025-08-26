using CimBios.Core.CimModel.CimDatatypeLib;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace CimBios.Core.CimModel.Validation.Script;

public class ScriptValidationRule : ValidationRuleBase
{
    public string Code { get; set; } = string.Empty;

    public override IEnumerable<IValidationResult> Execute(
        IReadOnlyModelObject modelObject)
    {
        var options = ScriptOptions.Default.
            WithReferences(AssemblyInfo.References).
            WithImports(AssemblyInfo.Usings);
        
        var globals = new ScriptGlobals { ModelObject = modelObject };

        try
        {
            var state = CSharpScript.RunAsync(Code, 
                options, globals).Result;
        
            if (state.ReturnValue is not IEnumerable<IValidationResult> 
                validationResults)
            {
                throw new InvalidDataException();
            }
            
            return validationResults;
        }
        catch (Exception e)
        {
            return [new ScriptExceptionValidationResult { Message = e.Message }];
        }
    }

    public override bool NeedExecute(IReadOnlyModelObject modelObject) => true;
}

public class ScriptExceptionValidationResult : IValidationResult
{
    public ValidationResultKind ResultType => ValidationResultKind.Fail;
    
    public required string Message { get; init; }
}
