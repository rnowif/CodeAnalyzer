using System.Collections.Generic;
using System.Linq;
using CodeAnalyzer.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalyzer.Methods
{
    public class MethodGraph
    {
        private readonly IReadOnlyCollection<MethodConnection> _connections;
        private readonly IReadOnlyCollection<MethodDeclarationSyntax> _methods;

        public int CountVisible => _methods.Count;
        public int CountDirectConnections => _connections.Count(c => c.Type == MethodConnection.ConnectionType.Direct);

        private MethodGraph(IReadOnlyCollection<MethodConnection> connections, IReadOnlyCollection<MethodDeclarationSyntax> methods)
        {
            _connections = connections;
            _methods = methods;
        }

        public static MethodGraph FromClass(ClassAnalyzer @class)
        {
            // Consider all public methods (excluding constructors) and fields/properties
            // Methods A and B are directly connected if:
            // - They both access the same field/property, or
            // - The call trees starting at A and B access the same field/property.

            var connections = new List<MethodConnection>();
            var visibleMethods = new List<MethodDeclarationSyntax>();

            var variablesAccessedByMethods = new Dictionary<ISymbol, List<MethodDeclarationSyntax>>();

            // Building a reverse-index of variables accessed by methods
            var classLevelVariables = GetClassLevelVariables(@class);
            foreach (var method in @class.Syntax.Members.OfType<MethodDeclarationSyntax>().Where(IsVisible))
            {
                visibleMethods.Add(method);

                var accessedVariables = method.DescendantNodes().OfType<IdentifierNameSyntax>()
                    .Select(s => @class.SemanticModel.GetSymbolInfo(s).Symbol)
                    .Where(s => s != null && classLevelVariables.Any(s.Equals));

                foreach (var variable in accessedVariables)
                {
                    if (!variablesAccessedByMethods.ContainsKey(variable!))
                    {
                        variablesAccessedByMethods[variable] = new List<MethodDeclarationSyntax>();
                    }

                    variablesAccessedByMethods[variable].Add(method);
                }
            }

            // Marking all methods that share a variable as directly connected

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator - Clearer this way
            foreach (var (_, methods) in variablesAccessedByMethods)
            {
                var directConnections = methods
                    .SelectMany(x => methods, (x, y) => new MethodConnection(MethodConnection.ConnectionType.Direct, x, y))
                    .Where(connection => !connection.IsSelfConnected)
                    .Distinct(new MethodConnectionComparer());

                connections.AddRange(directConnections);
            }

            return new MethodGraph(connections, visibleMethods);
        }

        private static IEnumerable<ISymbol> GetClassLevelVariables(ClassAnalyzer @class)
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

        private static bool IsVisible(MemberDeclarationSyntax method)
        {
            return method.Modifiers.All(modifier => modifier.Text != "private");
        }
    }
}
