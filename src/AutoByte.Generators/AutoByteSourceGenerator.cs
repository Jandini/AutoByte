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

                var endian = autoByteAttribute.IsBigEndian ? "BigEndian" : "LittleEndian";

                var classAccessibility = classSymbol.DeclaredAccessibility switch
                {
                    Accessibility.Public => "public",
                    Accessibility.Internal => "internal",
                    Accessibility.Private => "private",
                    _ => string.Empty
                };

                var computedStructureSize = 0;

                foreach (var property in properties)
                {
                    var propertyName = property.Name;

                    if (string.IsNullOrEmpty(propertyName))
                        continue;


                    var fieldAttribute = property.GetAttribute<AutoByteStringAttribute>()
                        ?? property.GetAttribute<AutoByteFieldAttribute>();

                    if (fieldAttribute != null && fieldAttribute.Skip > 0)
                    {
                        codeBuilder.AppendLine($"{" ",12}slide.Skip({fieldAttribute.Skip});");
                        computedStructureSize += fieldAttribute.Skip;
                    }
                       
                    // Get ByteSlide method and field size base on type or given size in the attribute
                    var methodInfo = GetMethodInfo(property, endian, fieldAttribute);

                    // Compute structure size
                    computedStructureSize += methodInfo.Item2;

                    codeBuilder.AppendLine($"{" ",12}{propertyName} = slide.{methodInfo.Item1};");
                }

                // Add using Autobyte
                usingsBuilder.AppendLine("using AutoByte;");
                
                // Add using System.Text if any of the properties is string
                if (properties.Any(x => x.Type.ToString() == "string"))
                    usingsBuilder.AppendLine("using System.Text;");


                string generatedCode = codeBuilder.ToString().TrimEnd('\r', '\n');

                // Do not return computed size due to SizeFromProperty.
                var structureSize = autoByteAttribute.Size > 0 ? autoByteAttribute.Size : 0;
                var className = classDeclaration.Identifier.ToString();
                var namespaceName = classDeclaration.GetNamespace();

                var source = GenerateDeserializeImplementation(usingsBuilder.ToString(), classAccessibility, namespaceName, className, structureSize, generatedCode);
                context.AddSource($"{className}.g.cs", SourceText.From(source, Encoding.UTF8));
            }
        }


        private Tuple<string, int> GetMethodInfo(IPropertySymbol property, string endian, AutoByteFieldAttribute fieldAttribute)
        {

            var propertyType = property.Type.ToString();
            var enumType = string.Empty;

            // For Enum types get underlaying property type
            if (property.Type.TypeKind == TypeKind.Enum)
            {
                enumType = $"<{propertyType}>";
                propertyType = (property.Type as INamedTypeSymbol).EnumUnderlyingType.ToDisplayString();
            }

            string method = propertyType switch
            {
                "byte" => $"GetByte{enumType}()",
                "short" => $"GetInt16{endian}{enumType}()",
                "int" => $"GetInt32{endian}{enumType}()",
                "long" => $"GetInt64{endian}{enumType}()",
                "ushort" => $"GetUInt16{endian}{enumType}()",
                "uint" => $"GetUInt32{endian}{enumType}()",
                "ulong" => $"GetUInt64{endian}{enumType}()",
                "byte[]" => $"Slide({fieldAttribute?.SizeFromProperty ?? fieldAttribute.Size.ToString()}).ToArray()",
                "string" => GetStringMethodName(property, fieldAttribute), 
                _ => throw new Exception($"AutoByte code generator does not support {propertyType}."),
            };

            int size = propertyType switch
            {
                "byte" => 1,
                "short" => 2,
                "ushort" => 2,
                "int" => 4,
                "uint" => 4,
                "long" => 8,
                "ulong" => 8,
                _ => fieldAttribute?.Size ?? 0
            };

            return new Tuple<string, int>(method, size);
        }

        private string GetStringMethodName(IPropertySymbol property, AutoByteFieldAttribute fieldAttribute)
        {            
            
            if (fieldAttribute is AutoByteStringAttribute stringField)
            {
                var stringFieldSize = fieldAttribute?.SizeFromProperty ?? fieldAttribute.Size.ToString();

                if (stringField.Encoding != null)
                {
                    return stringField.Encoding switch
                    {
                        "UTF8" => $"GetUtf8String({stringFieldSize})",
                        "Unicode" => $"GetString(Encoding.Unicode, {stringFieldSize})",
                        "BigEndianUnicode" => $"GetString(Encoding.BigEndianUnicode, {stringFieldSize})",
                        "UTF7" => $"GetString(Encoding.UTF7, {stringFieldSize})",
                        "UTF32" => $"GetString(Encoding.UTF32, {stringFieldSize})",
                        _ => throw new Exception($"Encoding {stringField.Encoding} given in AutoByteStringAttribute for {property.Name} is not supported.")
                    };
                }
                else 
                {
                    if (stringField.CodePage > 0)
                        return $"GetString(Encoding.GetEncoding({stringField.CodePage}), {stringFieldSize})";
                    else
                        return $"GetUtf8String({stringFieldSize})";
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
