using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Diagnostics;
using System.Reflection;
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
                AutoByteStructureAttribute autoByteAttribute = classSymbol.GetAttributes()
                    .FirstOrDefault(x => x.AttributeClass.Name == "AutoByteStructureAttribute")
                    .ToInstance<AutoByteStructureAttribute>();

                if (autoByteAttribute == null)
                    continue;

                // Get the properties of the class
                var properties = classSymbol.GetMembers()
                    .Where(x => x.Kind == SymbolKind.Property)
                    .Select(x => (IPropertySymbol)x);

                var codeBuilder = new StringBuilder();
                var endian = autoByteAttribute.IsBigEndian ? "BigEndian" : "LittleEndian";

                foreach (var property in properties)
                {
                    var propertyName = property.Name;
                    var methodName = GetMethodName(property, endian);                   

                    if (string.IsNullOrEmpty(propertyName))
                        continue;

                    codeBuilder.AppendLine($"{" ",12}{propertyName} = slide.{methodName};");
                }
                
                string generatedCode = codeBuilder.ToString().TrimEnd('\r', '\n');
                var structureSize = autoByteAttribute.Size;
                var className = classDeclaration.Identifier.ToString();
                var namespaceName = classDeclaration.GetNamespace();

                var source = GenerateDeserializeImplementation(namespaceName, className, structureSize, generatedCode);
                context.AddSource($"{className}.g.cs", SourceText.From(source, Encoding.UTF8));
            }
        }


        private string GetMethodName(IPropertySymbol property, string endian)
        {
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
                "byte" => $"GetByte{enumType}()",
                "short" => $"GetInt16{endian}{enumType}()",
                "int" => $"GetInt32{endian}{enumType}()",
                "long" => $"GetInt64{endian}{enumType}()",
                "ushort" => $"GetUInt16{endian}{enumType}()",
                "uint" => $"GetUInt32{endian}{enumType}()",
                "ulong" => $"GetUInt64{endian}{enumType}()",
                // "string" => GetStringMethodName(property), 
                _ => throw new Exception($"AutoByte code generator does not support {propertyType}."),
            };
            return method;

        }

        private string GetStringMethodName(IPropertySymbol property)
        {
            throw new NotImplementedException();

            //AutoByteFieldAttribute fieldAttribute = null;

            //var attributes = property.GetAttributes();

            //if (attributes != null && attributes.Length > 0)
            //{
            //    fieldAttribute = attributes.GetAttribute<AutoByteFieldAttribute>();

            //    fieldAttribute = attributes
            //        .FirstOrDefault(a => a.AttributeClass.Name == "AutoByteFieldAttribute")
            //        .ToInstance<AutoByteFieldAttribute>();
            //}


            //return fieldAttribute != null ? $"GetUtf8String({fieldAttribute.Size})" : throw new Exception($"{property.Name} is missing AutoByteString attribute."),
        }

        private string GenerateDeserializeImplementation(string namespaceName, string className, int structureSize, string generatedCode)
        {
            return $@"using AutoByte;

{(string.IsNullOrEmpty(namespaceName) ? null : $@"namespace {namespaceName}
{{")}
    public partial class {className} : IByteStructure
    {{
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
