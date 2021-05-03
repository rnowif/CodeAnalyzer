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
        public static IEnumerable<string> FindDependencies(this SourceTree tree, ClassDeclaration declaration)
        {
            // A "dependency" is any other class that our class is coupled with, i.e.
            // - a class that our class relies on
            // - a class that relies on our class
            // It can be via attributes (composition), associations, local variables, instantiations or injected dependencies (arguments to methods)
            // A class must be counted only once

            var dependentTypes = new HashSet<ITypeSymbol>();

            var semanticModel = tree.FindModel(declaration);

            // Constructors
            foreach (var methodSyntax in declaration.Syntax.Members.OfType<ConstructorDeclarationSyntax>())
            {
                var methodSymbol = semanticModel.GetDeclaredSymbol(methodSyntax) ?? throw new Exception();
                foreach (var type in methodSymbol.Parameters.SelectMany(p => p.Type.Expand().Where(IsAnalyzed)))
                {
                    dependentTypes.Add(type);
                }

                foreach (var type in methodSymbol.ReturnType.Expand().Where(IsAnalyzed))
                {
                    dependentTypes.Add(type);
                }
            }

            // Methods
            foreach (var methodSyntax in declaration.Syntax.Members.OfType<MethodDeclarationSyntax>())
            {
                var methodSymbol = semanticModel.GetDeclaredSymbol(methodSyntax) ?? throw new Exception();
                foreach (var type in methodSymbol.Parameters.SelectMany(p => p.Type.Expand().Where(IsAnalyzed)))
                {
                    dependentTypes.Add(type);
                }

                foreach (var type in methodSymbol.ReturnType.Expand().Where(IsAnalyzed))
                {
                    dependentTypes.Add(type);
                }
            }

            // Fields
            foreach (var variableDeclaratorSyntax in declaration.Syntax.Members.OfType<FieldDeclarationSyntax>()
                .SelectMany(s => s.Declaration.Variables))
            {
                var symbol = semanticModel.GetDeclaredSymbol(variableDeclaratorSyntax) as IFieldSymbol ??
                             throw new Exception();
                foreach (var type in symbol.Type.Expand().Where(IsAnalyzed))
                {
                    dependentTypes.Add(type);
                }
            }

            // Properties
            foreach (var propertyDeclarationSyntax in declaration.Syntax.Members.OfType<PropertyDeclarationSyntax>())
            {
                var symbol = semanticModel.GetDeclaredSymbol(propertyDeclarationSyntax) ?? throw new Exception();
                foreach (var type in symbol.Type.Expand().Where(IsAnalyzed))
                {
                    dependentTypes.Add(type);
                }
            }

            // Instantiations
            foreach (var creationExpressionSyntax in declaration.Syntax.DescendantNodes()
                .OfType<ObjectCreationExpressionSyntax>())
            {
                var symbol = semanticModel.GetTypeInfo(creationExpressionSyntax);
                foreach (var type in symbol.Type.Expand().Where(IsAnalyzed))
                {
                    dependentTypes.Add(type);
                }
            }

            // Inheritance

            // Usages


            return dependentTypes.Select(t => t.ToString() ?? "");
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
