using CimBios.Core.CimModel.Schema;
using CimBios.Tools.CimTypeLibBuilder.TemplateReader;

namespace CimBios.Tools.CimTypeLibBuilder.CodeBuilder;

internal class CodeBuilder (ICimSchema cimSchema, 
    CodeBlockSyntax[] blockSyntaxes, string namespaceName)
{
    public void Compile(string filePath)
    {
        var writer = new StreamWriter(filePath);

        if (_syntaxBlocks.TryGetValue(TemplateReader.TemplateReader.RootBlock, 
            out var rootSyntax) == false)
        {
            throw new Exception("Compilation failed. There is no root syntax block!");
        }

        if (rootSyntax.References.Contains(NamespaceBlock) == false)
        {
            throw new Exception("Compilation failed. Namespace block reference in root block expected!");
        }

        string code = rootSyntax.Text;
        var compiledRef = CompileNamespace();
        code = code.Replace($"&:{NamespaceBlock}:", compiledRef);

        writer.Write(code);
        writer.Close();
    }

    private string CompileNamespace()
    {
        if (_syntaxBlocks.TryGetValue(NamespaceBlock, 
            out var namespaceSyntax) == false)
        {
            throw new Exception("Compilation failed. Namespace block has not been defined!");
        }

        var binder = new NamespaceBinder() { ns = _namespaceName };
        var compliedNamespace = CompileWithVariables(namespaceSyntax, binder);

        if (namespaceSyntax.References.Contains(ClassBlock))
        {
            var extensions = _cimSchema.Extensions;
            var classes = _cimSchema.Classes.Where(
                c => !c.IsEnum && !c.IsDatatype && !extensions.Contains(c));

            var compiledClasses = CompileClasses(classes);
            compliedNamespace = compliedNamespace
                .Replace($"&:{ClassBlock}:", compiledClasses);
        }

        if (namespaceSyntax.References.Contains(EnumBlock))
        {
            var enums = _cimSchema.Classes.Where(c => c.IsEnum && !c.IsExtension);

            var compiledEnums = CompileEnums(enums);
            compliedNamespace = compliedNamespace
                .Replace($"&:{EnumBlock}:", compiledEnums);
        }

        return compliedNamespace;
    }

    private string CompileAnnotation(ICimMetaResource cimMetaResource)
    {
        if (_syntaxBlocks.TryGetValue(AnnotationBlock, 
            out var annotationSyntax) == false)
        {
            throw new Exception("Compilation failed. Annotation block has not been defined!");
        }

        return CompileWithVariables(annotationSyntax, 
            new AnnotationBinder() { MappedObject = cimMetaResource });
    }

    private string CompileClasses(IEnumerable<ICimMetaClass> classes)
    {
        if (_syntaxBlocks.TryGetValue(ClassBlock, 
            out var classSyntax) == false)
        {
            throw new Exception("Compilation failed. Class block has not been defined!");
        }

        var compiledClasses = string.Empty;

        foreach (var metaClass in classes)
        {
            var compiledClass = CompileWithVariables(classSyntax, 
                new ClassBinder() { MappedObject = metaClass });

            if (classSyntax.References.Contains(AnnotationBlock))
            {
                compiledClass = compiledClass.Replace(
                    $"&:{AnnotationBlock}:", CompileAnnotation(metaClass));
            }

            if (classSyntax.References.Contains(AttributeBlock))
            {
                compiledClass = compiledClass.Replace(
                    $"&:{AttributeBlock}:", CompileProperties(metaClass, 
                        AttributeBlock, CimMetaPropertyKind.Attribute));
            }

            if (classSyntax.References.Contains(Property1Block))
            {
                compiledClass = compiledClass.Replace(
                    $"&:{Property1Block}:", CompileProperties(metaClass,
                        Property1Block, CimMetaPropertyKind.Assoc1To1));
            }

            if (classSyntax.References.Contains(PropertyMBlock))
            {
                compiledClass = compiledClass.Replace(
                    $"&:{PropertyMBlock}:", CompileProperties(metaClass,
                        PropertyMBlock, CimMetaPropertyKind.Assoc1ToM));
            }

            compiledClasses += compiledClass;
        }

        return compiledClasses;
    }

    private string CompileEnums(IEnumerable<ICimMetaClass> classes)
    {
        if (_syntaxBlocks.TryGetValue(EnumBlock, 
            out var enumSyntax) == false)
        {
            throw new Exception("Compilation failed. Enum block has not been defined!");
        }

        var compiledEnums = string.Empty;

        foreach (var metaClass in classes)
        {
            var compiledEnum = CompileWithVariables(enumSyntax, 
                new EnumBinder() { MappedObject = metaClass });

            if (enumSyntax.References.Contains(AnnotationBlock))
            {
                compiledEnum = compiledEnum.Replace(
                    $"&:{AnnotationBlock}:", CompileAnnotation(metaClass));
            }

            if (enumSyntax.References.Contains(EnumValueBlock))
            {
                compiledEnum = compiledEnum.Replace(
                    $"&:{EnumValueBlock}:", CompileEnumValues(metaClass));
            }

            compiledEnums += compiledEnum;
        }

        return compiledEnums;
    }

    private string CompileEnumValues(ICimMetaClass metaEnum)
    {
        if (_syntaxBlocks.TryGetValue(EnumValueBlock, 
            out var propSyntax) == false)
        {
            throw new Exception($"Compilation failed. Enumn value block has not been defined!");
        }

        var enumValueList = _cimSchema.GetClassIndividuals(metaEnum, true);

        var compiledEnumValues = string.Empty;

        foreach (var metaIndividual in enumValueList)
        {
            var compiledValue = CompileWithVariables(propSyntax, 
                new EnumValueBinder() { MappedObject = metaIndividual });

            if (propSyntax.References.Contains(AnnotationBlock))
            {
                compiledValue = compiledValue.Replace(
                    $"&:{AnnotationBlock}:", CompileAnnotation(metaIndividual));
            }

            compiledEnumValues += compiledValue;
        }

        return compiledEnumValues;
    }

    private string CompileProperties(ICimMetaClass ownerClass, 
        string blockClassName, CimMetaPropertyKind propertyKind)
    {
        if (_syntaxBlocks.TryGetValue(blockClassName, 
            out var propSyntax) == false)
        {
            throw new Exception($"Compilation failed. Property block {blockClassName} has not been defined!");
        }

        var propsList = ownerClass.AllProperties.Where(
            p => p.PropertyKind == propertyKind
            && (p.OwnerClass == ownerClass 
                || ownerClass.Extensions.Contains(p.OwnerClass))
        );

        var compiledProps = string.Empty;
        foreach (var metaProp in propsList)
        {
            var compiledProp = CompileWithVariables(propSyntax, 
                new PropertyBinder() { MappedObject = metaProp });

            if (propSyntax.References.Contains(AnnotationBlock))
            {
                compiledProp = compiledProp.Replace(
                    $"&:{AnnotationBlock}:", CompileAnnotation(metaProp));
            }

            compiledProps += compiledProp;
        }

        return compiledProps;
    }

    private string CompileWithVariables(CodeBlockSyntax codeSyntax, 
        ISchemaObjectBinder binder)
    {
        var complied = codeSyntax.Text;

        foreach (var variable in codeSyntax.Variables)
        {
            var value = binder.GetVariableValue(variable).ReplaceLineEndings("");
            complied = complied.Replace($"$:{variable}:", value);
        }

        return complied;
    }

    private readonly ICimSchema _cimSchema = cimSchema;
    private readonly Dictionary<string, CodeBlockSyntax> _syntaxBlocks 
        = blockSyntaxes.ToDictionary(k => k.ClassName, v => v);
    private readonly string _namespaceName = namespaceName;

    private static string NamespaceBlock = "namespace";
    private static string AnnotationBlock = "annotation";
    private static string ClassBlock = "class";
    private static string AttributeBlock = "attribute";
    private static string Property1Block = "property1";
    private static string PropertyMBlock = "propertyM";
    private static string EnumBlock = "enum";
    private static string EnumValueBlock = "enum_value";
}
