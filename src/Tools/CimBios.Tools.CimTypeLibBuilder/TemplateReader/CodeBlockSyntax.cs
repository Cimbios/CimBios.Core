namespace CimBios.Tools.CimTypeLibBuilder.TemplateReader;

internal class CodeBlockSyntax(string className)
{
    public string ClassName { get; set; } = className;
    public string Text { get; set; } = string.Empty;
    public HashSet<string> Variables { get; } = [];
    public HashSet<string> References { get; } = [];
}