using System.CommandLine;
using System.CommandLine.Builder;
using CimBios.Core.CimModel.Schema.RdfSchema;
using CimBios.Tools.CimTypeLibBuilder.CodeBuilder;
using CimBios.Tools.CimTypeLibBuilder.TemplateReader;

// ----------------------------------------------------------------------------

#region Command line args reading
var schemaPathArgument = new Argument<string>
    ("schema-file", "The input schema file to process");
var templatePathArgument = new Argument<string>
    ("template-file", "The input code template file to process");
var namespaceOption = new Option<string>
    ("-namespace", "The input schema file to process") { IsRequired = true };
var serializerOption = new Option<SerializerTypeInfo>("-serializer", 
        () => SerializerTypeInfo.RDFS, "The input schema serializer type");

var rootCommand = new RootCommand
{
    schemaPathArgument,
    templatePathArgument,
    namespaceOption,
    serializerOption,
};

var rootCommandParser = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .Build();

var parseResult = rootCommandParser.Parse(args);
#endregion Command line args reading

#region Schema reading
var schemaPath = parseResult.GetValueForArgument(schemaPathArgument);
var cimSchema = new CimRdfSchemaXmlFactory().CreateSchema();
cimSchema.Load(new StreamReader(schemaPath));
#endregion Schema reading

#region Template reading
var templatePath = parseResult.GetValueForArgument(templatePathArgument); 
var templateCodeBlocks = TemplateReader.ReadTemplate(templatePath);
#endregion Template reading

#region Typelib compilation
var namespaceOptionValue = parseResult.GetValueForOption(namespaceOption); 
var codePath = templatePath.Replace(Path.GetExtension(templatePath), ".blc");
var codeBuilder = new CodeBuilder(cimSchema, templateCodeBlocks.ToArray(), 
    namespaceOptionValue!);
codeBuilder.Compile(codePath);
#endregion Typelib compilation

// ----------------------------------------------------------------------------
public enum SerializerTypeInfo
{
    Auto,
    RDFS,
}
