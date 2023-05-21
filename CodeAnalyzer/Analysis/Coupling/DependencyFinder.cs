using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalyzer.Analysis.Coupling;

public class DependencyFinder : CSharpSyntaxRewriter
{
    private readonly List<ITypeSymbol> _dependencies = new();

    private readonly SemanticModel _semanticModel;
    private readonly ClassDeclarationSyntax _syntax;

    public DependencyFinder(ClassAnalyzer @class)
    {
        _semanticModel = @class.SemanticModel;
        _syntax = @class.Syntax;
    }

    public IEnumerable<ISymbol?> FindDependencies()
    {
        Visit(_syntax);

        return _dependencies.ToHashSet(SymbolEqualityComparer.Default);
    }

    public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        var symbol = _semanticModel.GetDeclaredSymbol(node) ?? throw new Exception();

        _dependencies.AddRange(symbol.FindDependencies());

        return base.VisitConstructorDeclaration(node);
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        var symbol = _semanticModel.GetDeclaredSymbol(node) ?? throw new Exception();

        _dependencies.AddRange(symbol.FindDependencies());

        return base.VisitMethodDeclaration(node);
    }

    public override SyntaxNode? VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        var symbols = node.Declaration.Variables
            .Select(syntax => _semanticModel.GetDeclaredSymbol(syntax) as IFieldSymbol ?? throw new Exception());

        _dependencies.AddRange(symbols.SelectMany(symbol => symbol.FindDependencies()));

        return base.VisitFieldDeclaration(node);
    }

    public override SyntaxNode? VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        var symbol = _semanticModel.GetDeclaredSymbol(node) ?? throw new Exception();

        _dependencies.AddRange(symbol.FindDependencies());

        return base.VisitPropertyDeclaration(node);
    }

    public override SyntaxNode? VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        var typeInfo = _semanticModel.GetTypeInfo(node);

        _dependencies.AddRange(typeInfo.FindDependencies());

        return base.VisitObjectCreationExpression(node);
    }

    public override SyntaxNode? VisitBaseList(BaseListSyntax node)
    {
        var typeInfos = node.Types.Select(syntax => _semanticModel.GetTypeInfo(syntax.Type));

        _dependencies.AddRange(typeInfos.SelectMany(symbol => symbol.FindDependencies()));

        return base.VisitBaseList(node);
    }
}
