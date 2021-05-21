using System.Collections.Generic;
using System.Linq;
using CodeAnalyzer.Report;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalyzer.Analysis.Cohesion
{
    public static class MethodGraphExtensions
    {
        public static IEnumerable<ISymbol> GetClassLevelVariables(this ClassAnalyzer @class, AnalysisConfiguration configuration)
        {
            var fieldNodes = @class.Syntax.Members.OfType<FieldDeclarationSyntax>()
                .SelectMany(s => s.Declaration.Variables)
                .Cast<SyntaxNode>();

            var propertyNodes = @class.Syntax.Members.OfType<PropertyDeclarationSyntax>()
                .Cast<SyntaxNode>();

            return fieldNodes.Concat(propertyNodes)
                .Select(v => @class.SemanticModel.GetDeclaredSymbol(v))
                .Where(s => s != null)
                .Where(s => s is IFieldSymbol field && configuration.SelectField(field) || s is IPropertySymbol property && configuration.SelectProperty(property))
                .Select(s => s!);
        }
    }
}
