using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalyzer.Analysis.Cohesion
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
    }
}
