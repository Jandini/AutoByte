using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;
using System.Text;

namespace AutoByte
{
    [Generator]
    public class AutoByteSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var syntaxReceiver = (SyntaxReceiver)context.SyntaxReceiver;

            foreach (var classDeclaration in syntaxReceiver.CandidateClasses)
            {
                // Get the syntax tree for the class being generated
                SyntaxTree syntaxTree = context.Compilation.SyntaxTrees.FirstOrDefault(
                    x => x.FilePath == classDeclaration.GetLocation().SourceTree.FilePath);

                if (syntaxTree == null)
                    continue;

                SemanticModel semanticModel = context.Compilation.GetSemanticModel(syntaxTree);

                if (semanticModel == null)
                    continue;

                // Get the symbol for the class
                INamedTypeSymbol classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

                if (classSymbol == null)
                    continue;

                // Get the attribute and its property value
                var autoByteAttribute = classSymbol.GetAttribute<AutoByteStructureAttribute>();

                if (autoByteAttribute == null)
                    continue;

                // Get the properties of the class
                var properties = classSymbol.GetMembers()
                    .Where(x => x.Kind == SymbolKind.Property)
                    .Select(x => (IPropertySymbol)x);

                var codeBuilder = new StringBuilder();
                var usingsBuilder = new StringBuilder();

                var hasStrings = false;
                var endian = autoByteAttribute.IsBigEndian ? "BigEndian" : "LittleEndian";

                var classAccessibility = classSymbol.DeclaredAccessibility switch
                {
                    Accessibility.Public => "public",
                    Accessibility.Internal => "internal",
                    Accessibility.Private => "private",
                    _ => string.Empty
                };


                foreach (var property in properties)
                {
                    var propertyName = property.Name;
                    var methodName = GetMethodName(property, endian);                   

                    if (string.IsNullOrEmpty(propertyName))
                        continue;

                    if (!hasStrings && methodName.Contains("String"))
                        hasStrings = true;


                    codeBuilder.AppendLine($"{" ",12}{propertyName} = slide.{methodName};");
                }

                usingsBuilder.AppendLine("using AutoByte;");

                if (hasStrings)
                    usingsBuilder.AppendLine("using System.Text;");


                string generatedCode = codeBuilder.ToString().TrimEnd('\r', '\n');
                var structureSize = autoByteAttribute.Size;
                var className = classDeclaration.Identifier.ToString();
                var namespaceName = classDeclaration.GetNamespace();

                var source = GenerateDeserializeImplementation(usingsBuilder.ToString(), classAccessibility, namespaceName, className, structureSize, generatedCode);
                context.AddSource($"{className}.g.cs", SourceText.From(source, Encoding.UTF8));
            }
        }


        private string GetMethodName(IPropertySymbol property, string endian)
        {
            var fieldAttribute = property.GetAttribute<AutoByteStringAttribute>()
                ?? property.GetAttribute<AutoByteFieldAttribute>();

            var skip = GetSkipBytes(fieldAttribute);

            string propertyType = property.Type.ToString();
            string enumType = string.Empty;

            // For Enum types get underlaying property type
            if (property.Type.TypeKind == TypeKind.Enum)
            {
                enumType = $"<{propertyType}>";
                propertyType = (property.Type as INamedTypeSymbol).EnumUnderlyingType.ToDisplayString();
            }

            string method = propertyType switch
            {
                "byte" => $"{skip}GetByte{enumType}()",
                "short" => $"{skip}GetInt16{endian}{enumType}()",
                "int" => $"{skip}GetInt32{endian}{enumType}()",
                "long" => $"{skip}GetInt64{endian}{enumType}()",
                "ushort" => $"{skip}GetUInt16{endian}{enumType}()",
                "uint" => $"{skip}GetUInt32{endian}{enumType}()",
                "ulong" => $"{skip}GetUInt64{endian}{enumType}()",
                "byte[]" => $"{skip}Slide({fieldAttribute.Size}).ToArray()",
                "string" => GetStringMethodName(property, fieldAttribute, skip), 
                _ => throw new Exception($"AutoByte code generator does not support {propertyType}."),
            };

            return method;
        }

        private string GetSkipBytes(AutoByteFieldAttribute fieldAttribute)
        {
            return (fieldAttribute != null && fieldAttribute.Skip > 0)
                ? $"Skip({fieldAttribute.Skip})."
                : string.Empty;
        }

        private string GetStringMethodName(IPropertySymbol property, AutoByteFieldAttribute fieldAttribute, string skip)
        {            
            
            if (fieldAttribute is AutoByteStringAttribute stringField)
            {
                if (stringField.Encoding != null)
                {
                    return stringField.Encoding switch
                    {
                        "UTF8" => $"{skip}GetUtf8String({stringField.Size})",
                        "Unicode" => $"{skip}GetString(Encoding.Unicode, {stringField.Size})",
                        "BigEndianUnicode" => $"{skip}GetString(Encoding.BigEndianUnicode, {stringField.Size})",
                        "UTF7" => $"{skip}GetString(Encoding.UTF7, {stringField.Size})",
                        "UTF32" => $"{skip}GetString(Encoding.UTF32, {stringField.Size})",
                        _ => throw new Exception($"Encoding {stringField.Encoding} given in AutoByteStringAttribute for {property.Name} is not supported.")
                    };
                }
                else 
                {
                    if (stringField.CodePage > 0)
                        return $"{skip}GetString(Encoding.GetEncoding({stringField.CodePage}), {stringField.Size})";
                    else
                        return $"{skip}GetUtf8String({stringField.Size})";
                }
            }


            throw new Exception($"Propertry {property.Name} require AutoByteString attribute.");          
        }

        private string GenerateDeserializeImplementation(string usings, string classAccessibility, string namespaceName, string className, int structureSize, string generatedCode)
        {
            return $@"{usings}
{(string.IsNullOrEmpty(namespaceName) ? null : $@"namespace {namespaceName}
{{")}        
    {classAccessibility} partial class {className} : IByteStructure
    {{
        {(structureSize > 0 ? $@"public static int StructureSize = {structureSize};" : null)}
        public int Deserialize(ref ByteSlide slide)
        {{
{generatedCode}
            return {structureSize};
        }}
    }}
{(string.IsNullOrEmpty(namespaceName) ? null : $@"}}")}
";
        }


        private class SyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax classDeclaration)
                {
                    var autoByteStructureAttribute = classDeclaration.AttributeLists
                        .SelectMany(a => a.Attributes)
                        .FirstOrDefault(a => a.Name.ToString() == "AutoByteStructure");

                    if (autoByteStructureAttribute != null)
                    {
                        if (!classDeclaration.IsPartial())
                            throw new Exception("Use partial class with AutoByteStructure attribute."); 

                        CandidateClasses.Add(classDeclaration);
                    }
                }
            }
        }
    }
}
