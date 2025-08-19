namespace CimBios.Tools.CimTypeLibBuilder.TemplateReader;

internal static class TemplateReader
{
    public static string RootBlock = "root";

    public static IEnumerable<CodeBlockSyntax> ReadTemplate(string templatePath)
    {
        var reader = new StreamReader(templatePath);

        var cursor = RootBlock;
        var rootCodeBlock = new CodeBlockSyntax(RootBlock);

        var blocksCache = new Dictionary<string, CodeBlockSyntax>
        {
            { rootCodeBlock.ClassName, rootCodeBlock }
        };

        var lineNumber = 0;
        do
        {
            ++lineNumber;
            var line = reader.ReadLine();

            if (line == null || line == string.Empty)
            {
            }
            else if (line.Trim() == "~")
            {
                blocksCache[cursor].Text += "\n";
            }
            else if (line.Trim().Length > 3 && line.Trim()[..2] == "@:"
                                            && line.Trim().Last() == ':')
            {
                var className = line.Trim()[2..(line.Trim().Length - 1)];

                if (cursor == RootBlock)
                {
                    var definition = new CodeBlockSyntax(className);
                    blocksCache.Add(definition.ClassName, definition);
                    cursor = className;
                }
                else if (className == cursor)
                {
                    blocksCache[cursor].Text = blocksCache[cursor].Text[..^1];

                    cursor = RootBlock;
                }
                else
                {
                    throw new Exception("Unexpected class definition in line " + lineNumber);
                }
            }
            else
            {
                blocksCache[cursor].Text += line + "\n";
                var variables = ParseSpecs(line, '$', lineNumber);
                foreach (var variable in variables.Distinct())
                {
                    if (blocksCache[cursor].Variables.Contains(variable)) continue;

                    blocksCache[cursor].Variables.Add(variable);
                }

                var refs = ParseSpecs(line, '&', lineNumber);
                foreach (var refName in refs.Distinct())
                {
                    if (blocksCache[cursor].References.Contains(refName)) continue;

                    if (blocksCache.ContainsKey(refName) == false)
                        throw new Exception("Undefined class reference in line " + lineNumber);

                    blocksCache[cursor].References.Add(refName);
                }
            }
        } while (reader.EndOfStream == false);

        reader.Close();

        return blocksCache.Values;
    }

    private static string[] ParseSpecs(string line, char spec, int lineNumber)
    {
        var variables = new List<string>();
        var fid = 0;

        while ((fid = line.IndexOf(spec, fid)) != -1)
        {
            var closeId = line.IndexOf(":", fid + 2);
            if (closeId == -1)
                throw new Exception("Unexpected end of the line while variable definition in line" + lineNumber);

            variables.Add(line.Substring(fid + 2, closeId - fid - 2));
            fid = closeId;
        }

        return variables.ToArray();
    }
}