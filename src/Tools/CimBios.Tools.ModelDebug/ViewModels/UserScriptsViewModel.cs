using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AvaloniaEdit.Document;
using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace CimBios.Tools.ModelDebug.ViewModels;

public class UserScriptsViewModel : ViewModelBase
{
    public TextDocument Code
    {
        get => _code;
        set
        {
            if (_code == value) return;
            
            _code = value;
            OnPropertyChanged();
        }
    }

    public TextDocument Result
    {
        get => _result;
        private set
        {
            if (_result == value) return;
            
            _result = value;
            OnPropertyChanged();
        }
    }

    public TextDocument CodeDocument { get; set; }

    private static Assembly[] References { get; } =
    [
        typeof(object).Assembly,
        typeof(Uri).Assembly,
        typeof(Enumerable).Assembly,
        typeof(IModelObject).Assembly,
        typeof(ModelObject).Assembly,
        typeof(ICimDataModel).Assembly,
    ];
    
    private static string[] Usings { get; } =
    [
        "System",
        "System.Collections.Generic",
        "System.Linq",
        "CimBios.Core.CimModel.CimDatatypeLib",
        "CimBios.Core.CimModel.DataModel",
        "CimBios.Core.CimModel.CimDatatypeLib.CIM17Types",
    ];

    public async Task Execute()
    {
        if (GlobalServices.LoaderService.DataContext == null) return;
        
        var options = ScriptOptions.Default.
            WithReferences(References).
            WithImports(Usings);

        try
        {
            var state = await CSharpScript.RunAsync(Code.Text, 
                options, GlobalServices.LoaderService.DataContext);
            
            Result.Text += $"\n\n>>> {state.ReturnValue}";
            OnPropertyChanged(nameof(Result));
        }
        catch (Exception e)
        {
            Result.Text += $"\n\n>>> {e.Message}";
            OnPropertyChanged(nameof(Result));
        }
    }

    public void ClearResult()
    {
        Result.Text = "";
        OnPropertyChanged(nameof(Result));
    }
    
    private TextDocument _code = new("");
    private TextDocument _result = new("");
}