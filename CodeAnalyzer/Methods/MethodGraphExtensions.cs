using System.Collections.Generic;
using System.Linq;
using CodeAnalyzer.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalyzer.Methods
{
    public static class MethodGraphExtensions
    {
        public static IEnumerable<ISymbol> GetClassLevelVariables(this ClassAnalyzer @class)
        {
            var fieldNodes = @class.Syntax.Members.OfType<FieldDeclarationSyntax>()
                .SelectMany(s => s.Declaration.Variables)
                .Cast<SyntaxNode>();

            var propertyNodes = @class.Syntax.Members.OfType<PropertyDeclarationSyntax>()
                .Cast<SyntaxNode>();

            return fieldNodes.Concat(propertyNodes)
                .Select(v => @class.SemanticModel.GetDeclaredSymbol(v))
                .Where(s => s != null)
                .Select(s => s!);
        }

        public static IEnumerable<IMethodSymbol> GetCallTree(this IMethodSymbol method, IReadOnlyDictionary<IMethodSymbol, IEnumerable<IMethodSymbol>> methodsByMethods)
        {
            var subMethods = new List<IMethodSymbol>();
            foreach (var subMethod in methodsByMethods[method])
            {
                subMethods.Add(subMethod);
                subMethods.AddRange(subMethod.GetCallTree(methodsByMethods));
            }

            return subMethods;
        }
    }
}
