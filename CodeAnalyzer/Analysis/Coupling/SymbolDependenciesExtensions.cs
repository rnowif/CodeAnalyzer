using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CodeAnalyzer.Analysis.Coupling;

public static class SymbolDependenciesExtensions
{
    public static IEnumerable<ITypeSymbol> FindDependencies(this IMethodSymbol method) =>
        method.Parameters.SelectMany(p => p.Type.Expand().Where(IsAnalyzed))
            .Concat(method.ReturnType.FindDependencies());

    public static IEnumerable<ITypeSymbol> FindDependencies(this IFieldSymbol field) => field.Type.FindDependencies();

    public static IEnumerable<ITypeSymbol> FindDependencies(this IPropertySymbol property) => property.Type.FindDependencies();

    public static IEnumerable<ITypeSymbol> FindDependencies(this TypeInfo type) => type.Type.FindDependencies();

    private static IEnumerable<ITypeSymbol> FindDependencies(this ITypeSymbol? symbol) => symbol.Expand().Where(IsAnalyzed);

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

    private static bool IsAnalyzed(ISymbol type)
    {
        var namespaceSymbol = type.ContainingNamespace;

        if (namespaceSymbol == null)
        {
            return false;
        }

        return !namespaceSymbol.IsGlobalNamespace && !namespaceSymbol.ToDisplayString().StartsWith("System");
    }
}
