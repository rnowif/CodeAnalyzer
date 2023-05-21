using System.Linq;
using Microsoft.CodeAnalysis;

namespace CodeAnalyzer.Analysis;

public static class MethodSymbolExtensions
{
    public static bool IsInterfaceImplementation(this IMethodSymbol method) => method.ContainingType
        .AllInterfaces
        .SelectMany(@interface => @interface.GetMembers().OfType<IMethodSymbol>())
        .Any(interfaceMethod => method.ContainingType.FindImplementationForInterfaceMember(interfaceMethod)?.Equals(method, SymbolEqualityComparer.Default) ?? false);
}
