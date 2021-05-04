using System;
using System.Collections.Generic;
using System.Linq;
using CodeAnalyzer.Tree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalyzer.Metrics
{
    public static class DependencyAnalyzer
    {
        public static IEnumerable<string> FindDependencies(this ClassAnalyzer classAnalyzer)
        {
            // A "dependency" is any other class that our class is coupled with, i.e.
            // - a class that our class relies on
            // - a class that relies on our class
            // It can be via attributes (composition), associations, local variables, instantiations or injected dependencies (arguments to methods)
            // A class must be counted only once

            var dependentTypes = new List<ITypeSymbol>();
            var classDeclarationSyntax = classAnalyzer.Syntax;
            var semanticModel = classAnalyzer.SemanticModel;

            // Constructors
            foreach (var methodSyntax in classDeclarationSyntax.Members.OfType<ConstructorDeclarationSyntax>())
            {
                var methodSymbol = semanticModel.GetDeclaredSymbol(methodSyntax) ?? throw new Exception();
                dependentTypes.AddRange(methodSymbol.Parameters.SelectMany(p => p.Type.Expand().Where(IsAnalyzed)));
                dependentTypes.AddRange(methodSymbol.ReturnType.Expand().Where(IsAnalyzed));
            }

            // Methods
            foreach (var methodSyntax in classDeclarationSyntax.Members.OfType<MethodDeclarationSyntax>())
            {
                var methodSymbol = semanticModel.GetDeclaredSymbol(methodSyntax) ?? throw new Exception();
                dependentTypes.AddRange(methodSymbol.Parameters.SelectMany(p => p.Type.Expand().Where(IsAnalyzed)));
                dependentTypes.AddRange(methodSymbol.ReturnType.Expand().Where(IsAnalyzed));
            }

            // Fields
            foreach (var variableDeclaratorSyntax in classDeclarationSyntax.Members.OfType<FieldDeclarationSyntax>().SelectMany(s => s.Declaration.Variables))
            {
                var symbol = semanticModel.GetDeclaredSymbol(variableDeclaratorSyntax) as IFieldSymbol ?? throw new Exception();
                dependentTypes.AddRange(symbol.Type.Expand().Where(IsAnalyzed));
            }

            // Properties
            foreach (var propertyDeclarationSyntax in classDeclarationSyntax.Members.OfType<PropertyDeclarationSyntax>())
            {
                var symbol = semanticModel.GetDeclaredSymbol(propertyDeclarationSyntax) ?? throw new Exception();
                dependentTypes.AddRange(symbol.Type.Expand().Where(IsAnalyzed));
            }

            // Instantiations
            foreach (var creationExpressionSyntax in classDeclarationSyntax.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
            {
                var symbol = semanticModel.GetTypeInfo(creationExpressionSyntax);
                dependentTypes.AddRange(symbol.Type.Expand().Where(IsAnalyzed));
            }

            // Inheritance
            if (classDeclarationSyntax.BaseList != null)
            {
                foreach (var baseType in classDeclarationSyntax.BaseList.Types)
                {
                    var symbol = semanticModel.GetTypeInfo(baseType.Type);
                    dependentTypes.AddRange(symbol.Type.Expand().Where(IsAnalyzed));
                }
            }

            return dependentTypes.ToHashSet().Select(t => t.ToString() ?? "");
        }

        /// <summary>
        /// Return the type and - if it's a generic - all the other associated types <br/>
        /// - <code>string</code> will return <code>[string]</code> <br/>
        /// - <code>IEnumerable&lt;string&gt;</code> will return <code>[IEnumerable, string]</code> <br/>
        /// - <code>IEnumerable&lt;Func&lt;A,B&gt;&gt;</code> will return <code>[IEnumerable, Func, A, B]</code> <br/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns> </returns>
        private static IEnumerable<ITypeSymbol> Expand(this ITypeSymbol? type)
        {
            if (type == null)
            {
                return new List<ITypeSymbol>();
            }

            IEnumerable<ITypeSymbol> types = new[] {type};

            if (type is INamedTypeSymbol {IsGenericType: true} namedTypeSymbol)
            {
                types = types.Concat(namedTypeSymbol.TypeArguments.SelectMany(Expand));
            }

            return types;
        }

        private static bool IsAnalyzed(ITypeSymbol type)
        {
            var namespaceSymbol = type.ContainingNamespace;

            if (namespaceSymbol == null)
            {
                return false;
            }

            return !namespaceSymbol.IsGlobalNamespace && !namespaceSymbol.Name.StartsWith("System");
        }
    }
}
