using CimBios.CimModel;
using System.Text.RegularExpressions;

static internal class ModelDebug
{
    private static Dictionary<string, ISubProgram> SubPrograms
        = new Dictionary<string, ISubProgram>()
        {
            { "LoadSchema", new LoadSchemaSubProgram("LoadSchema") },
            { "LoadModel", new LoadModelSubProgram("LoadModel") }
        };

    public static void Main()
    {
        var objectDictionary = new Dictionary<string, object?>();

        string? line = "";

        while (line != null && line != "Exit")
        {
            Console.Write(">: ");
            line = Console.ReadLine();

            var splitted = Regex.Split(line, "(\"[^\"]+\"|[^\\s\"]+)")
                .Where(s => s.Trim().Length > 0).Select(s => s.Replace("\"", ""));
            if (splitted.Count() == 0)
            {
                Console.WriteLine("Invalid command format!");
            }

            var subProgramName = splitted.First();

            if (subProgramName == "Exit")
            {
                break;
            }

            if (SubPrograms.TryGetValue(subProgramName, out var subProgram))
            {
                if (subProgram is IVoidReturn voidSubProgram)
                {
                    voidSubProgram.InvokeAction(splitted.Skip(1));
                }
                else if (subProgram is IObjectReturn objectReturn)
                {
                    var result = objectReturn.InvokeFunc(splitted.Skip(1));
                    if (objectDictionary.ContainsKey(subProgramName))
                    {
                        objectDictionary[subProgramName] = result;
                    }
                    else
                    {
                        objectDictionary.Add(subProgramName, result);
                    }
                }
            }
            else 
            {
                Console.WriteLine("Unknown command!");
            }
        }
    }

    static IEnumerable<object>? ParseParams(IEnumerable<object> @params)
    {
        return null;
    }

}

internal interface ISubProgram
{
    public string InvokeName { get; set; } 
}

internal interface IObjectReturn
{
    public Func<IEnumerable<object>, object> InvokeFunc { get; set; }
}

internal interface IVoidReturn
{
    public Action<IEnumerable<object>> InvokeAction { get; set; }
}

internal sealed class LoadSchemaSubProgram : ISubProgram, IObjectReturn
{
    public LoadSchemaSubProgram(string invokeName)
    {
        InvokeName = invokeName;
    }

    public string InvokeName { get; set; }
    public Func<IEnumerable<object>, object> InvokeFunc { get; set; }
        = new Func<IEnumerable<object>, object>(
            (p) =>
            {
                var schema = new CimBios.CimModel.Schema.CimSchema();

                var reader = new StreamReader(p.Single() as string);
                schema.Load(reader);

                return schema;
            });
}

internal sealed class LoadModelSubProgram : ISubProgram, IObjectReturn
{
    public LoadModelSubProgram(string invokeName)
    {
        InvokeName = invokeName;
    }

    public string InvokeName { get; set; }
    public Func<IEnumerable<object>, object> InvokeFunc { get; set; }
        = new Func<IEnumerable<object>, object>(
            (p) =>
            {
                var model = new CimBios.CimModel.Context.ModelContext();
                model.Load(p.Single() as string);

                return model;
            });
}