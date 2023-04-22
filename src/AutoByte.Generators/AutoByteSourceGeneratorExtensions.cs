using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace AutoByte
{
    internal static class AutoByteSourceGeneratorExtensions
    {

        /// <summary>
        /// Check if class is marked as partial. 
        /// </summary>
        /// <param name="classDeclaration"></param>
        /// <returns></returns>
        public static bool IsPartial(this ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
        }


        public static string GetNamespace(this BaseTypeDeclarationSyntax syntax)
        {
            // Get the syntax node for the containing namespace declaration
            SyntaxNode containingNamespace = syntax.AncestorsAndSelf()
                .OfType<NamespaceDeclarationSyntax>().FirstOrDefault();

            if (containingNamespace != null)
            {
                // Get the namespace name from the syntax node
                return ((NamespaceDeclarationSyntax)containingNamespace).Name.ToString();
            }

            return null;
        }

        public static T GetAttribute<T>(this ISymbol symbol) where T : Attribute 
        {
            return symbol.GetAttributes()
                .FirstOrDefault(x => x.AttributeClass.Name == typeof(T).Name)
                .ToInstance<T>();
        }

        public static T ToInstance<T>(this AttributeData attributeData) where T : Attribute
        {
            if (attributeData == null) 
                return null;

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            T instance = Activator.CreateInstance<T>();

            // Set named arguments
            foreach (var argument in attributeData.NamedArguments)
            {
                var property = properties.FirstOrDefault(x => x.Name == argument.Key);
                
                if (property != null)
                {
                    var value = argument.Value.Value;
                    property.SetValue(instance, value);
                }
            }

            return instance;
        }
    }
}
