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
#if DEBUG_
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


                var structureSize = autoByteAttribute.Size;
                var className = classDeclaration.Identifier.ToString();
                var namespaceName = classDeclaration.GetNamespace();

                var source = GenerateDeserializeImplementation(namespaceName, className, structureSize);
                context.AddSource($"{className}.g.cs", SourceText.From(source, Encoding.UTF8));
            }
        }

        private string GenerateDeserializeImplementation(string namespaceName, string className, int structureSize)
        {
            return $@"using AutoByte;

{(string.IsNullOrEmpty(namespaceName) ? null : $@"namespace {namespaceName}
{{")}
    public partial class {className} : IByteStructure
    {{
        public int Deserialize(ref ByteSlide slide)
        {{
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

                    if (autoByteStructureAttribute != null && classDeclaration.IsPartial())
                    {
                        CandidateClasses.Add(classDeclaration);
                    }
                }
            }
        }
    }
}
