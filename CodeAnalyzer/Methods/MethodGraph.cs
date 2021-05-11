using System.Collections.Generic;
using System.Linq;
using CodeAnalyzer.Analyzer;
using Microsoft.CodeAnalysis;

namespace CodeAnalyzer.Methods
{
    public class MethodGraph
    {
        private readonly IReadOnlyCollection<MethodConnection> _connections;
        private readonly IReadOnlyCollection<IMethodSymbol> _methods;

        public int CountVisible => _methods.Count;
        public int CountDirectConnections => _connections.Count(c => c.Type == MethodConnection.ConnectionType.Direct);

        private MethodGraph(IEnumerable<MethodConnection> connections, IEnumerable<IMethodSymbol> methods)
        {
            _connections = connections.ToList();
            _methods = methods.ToList();
        }

        public static MethodGraph FromClass(ClassAnalyzer @class)
        {
            // Marking all methods that share a variable as directly connected
            // Methods A and B are directly connected if:
            // - They both access the same field/property, or
            // - The call trees starting at A and B access the same field/property.

            var connections = new List<MethodConnection>();
            var reverseIndex = ReverseVariableIndex.Build(@class);

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator - Clearer this way
            foreach (var (_, methods) in reverseIndex.MethodGroups)
            {
                var directConnections = methods
                    .SelectMany(x => methods, (x, y) => new MethodConnection(MethodConnection.ConnectionType.Direct, x, y))
                    .Where(connection => !connection.IsSelfConnected);

                connections.AddRange(directConnections);
            }

            return new MethodGraph(connections.Distinct(new MethodConnectionComparer()), reverseIndex.VisibleMethods);
        }
    }
}
