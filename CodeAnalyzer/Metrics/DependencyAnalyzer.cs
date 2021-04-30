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
            // We can exclude classes from Microsoft and System namespaces
            // PropertyDeclarationSyntax, ConstructorDeclarationSyntax, MethodDeclarationSyntax

            var dependentTypes = new HashSet<string>();

            var semanticModel = tree.FindModel(declaration);

            foreach (var syntax in declaration.Syntax.Members.OfType<ConstructorDeclarationSyntax>())
            {
                var symbol = semanticModel.GetDeclaredSymbol(syntax) ?? throw new Exception();
                foreach (var parameter in symbol.Parameters.Where(IsNotSystem))
                {
                    dependentTypes.Add(parameter.Type.ToString());
                }
            }

            foreach (var syntax in declaration.Syntax.Members.OfType<MethodDeclarationSyntax>())
            {
                var symbol = semanticModel.GetDeclaredSymbol(syntax) ?? throw new Exception();
                foreach (var parameter in symbol.Parameters.Where(IsNotSystem))
                {
                    dependentTypes.Add(parameter.Type.ToString());
                }
            }

            return dependentTypes;
        }


        // TODO: Count generic types as dependencies also
        private static bool IsNotSystem(IParameterSymbol p)
        {
            return !p.Type.ContainingNamespace.IsGlobalNamespace && !p.Type.ContainingNamespace.Name.StartsWith("System");
        }
    }
}
