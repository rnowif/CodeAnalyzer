using System;
using System.Collections.Generic;
using System.Linq;
using CodeAnalyzer.Tree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalyzer.Dependencies
{
    public static class DependencyAnalyzer
    {
        public static IEnumerable<string> FindDependencies(this ClassAnalyzer classAnalyzer)
        {
            var dependentTypes = new List<ITypeSymbol>();
            var classDeclarationSyntax = classAnalyzer.Syntax;
            var semanticModel = classAnalyzer.SemanticModel;

            dependentTypes.AddRange(FindDependenciesInConstructors(classDeclarationSyntax, semanticModel));
            dependentTypes.AddRange(FindDependenciesInMethods(classDeclarationSyntax, semanticModel));
            dependentTypes.AddRange(FindDependenciesInFields(classDeclarationSyntax, semanticModel));
            dependentTypes.AddRange(FindDependenciesInProperties(classDeclarationSyntax, semanticModel));
            dependentTypes.AddRange(FindDependenciesInBody(classDeclarationSyntax, semanticModel));
            dependentTypes.AddRange(FindDependenciesInBaseClasses(classDeclarationSyntax, semanticModel));

            return dependentTypes.ToHashSet().Select(t => t.ToString() ?? "");
        }

        private static IEnumerable<ITypeSymbol> FindDependenciesInConstructors(TypeDeclarationSyntax classDeclarationSyntax, SemanticModel semanticModel) =>
            classDeclarationSyntax.Members.OfType<ConstructorDeclarationSyntax>()
                .Select(syntax => semanticModel.GetDeclaredSymbol(syntax) ?? throw new Exception())
                .SelectMany(symbol => symbol.FindDependencies());

        private static IEnumerable<ITypeSymbol> FindDependenciesInMethods(TypeDeclarationSyntax classDeclarationSyntax, SemanticModel semanticModel) =>
            classDeclarationSyntax.Members.OfType<MethodDeclarationSyntax>()
                .Select(syntax => semanticModel.GetDeclaredSymbol(syntax) ?? throw new Exception())
                .SelectMany(symbol => symbol.FindDependencies());

        private static IEnumerable<ITypeSymbol> FindDependenciesInFields(TypeDeclarationSyntax classDeclarationSyntax, SemanticModel semanticModel) =>
            classDeclarationSyntax.Members.OfType<FieldDeclarationSyntax>()
                .SelectMany(s => s.Declaration.Variables)
                .Select(syntax => semanticModel.GetDeclaredSymbol(syntax) as IFieldSymbol ?? throw new Exception())
                .SelectMany(symbol => symbol.FindDependencies());

        private static IEnumerable<ITypeSymbol> FindDependenciesInProperties(TypeDeclarationSyntax classDeclarationSyntax, SemanticModel semanticModel) =>
            classDeclarationSyntax.Members.OfType<PropertyDeclarationSyntax>()
                .Select(syntax => semanticModel.GetDeclaredSymbol(syntax) ?? throw new Exception())
                .SelectMany(symbol => symbol.FindDependencies());

        private static IEnumerable<ITypeSymbol> FindDependenciesInBody(SyntaxNode classDeclarationSyntax, SemanticModel semanticModel) =>
            classDeclarationSyntax.DescendantNodes().OfType<ObjectCreationExpressionSyntax>()
                .Select(syntax => semanticModel.GetTypeInfo(syntax))
                .SelectMany(symbol => symbol.FindDependencies());

        private static IEnumerable<ITypeSymbol> FindDependenciesInBaseClasses(BaseTypeDeclarationSyntax classDeclarationSyntax, SemanticModel semanticModel) =>
            classDeclarationSyntax.BaseList?.Types
                .Select(syntax => semanticModel.GetTypeInfo(syntax.Type))
                .SelectMany(symbol => symbol.FindDependencies()) ?? new List<ITypeSymbol>();

    }
}
