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
        public int CountIndirectConnections => _connections.Count(c => c.Type == MethodConnection.ConnectionType.Indirect);

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

            // Methods A and B are indirectly connected if:
            // - They share a direct connection but are not directly connected themselves

            var connections = new List<MethodConnection>();
            var reverseIndex = ReverseVariableIndex.Build(@class);

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator - Clearer this way
            foreach (var (_, methods) in reverseIndex.MethodGroups)
            {
                connections.AddRange(ConnectAllMethods(methods, MethodConnection.ConnectionType.Direct));
            }

            var directConnectionByMethods = new Dictionary<IMethodSymbol, List<IMethodSymbol>>();
            foreach (var directConnection in connections)
            {
                if (!directConnectionByMethods.ContainsKey(directConnection.Method1))
                {
                    directConnectionByMethods[directConnection.Method1] = new List<IMethodSymbol>();
                }

                directConnectionByMethods[directConnection.Method1].Add(directConnection.Method2);

                if (!directConnectionByMethods.ContainsKey(directConnection.Method2))
                {
                    directConnectionByMethods[directConnection.Method2] = new List<IMethodSymbol>();
                }

                directConnectionByMethods[directConnection.Method2].Add(directConnection.Method1);
            }

            foreach (var (_, methods) in directConnectionByMethods)
            {
                var indirectlyConnectedMethods = ConnectAllMethods(methods, MethodConnection.ConnectionType.Indirect)
                    .Where(c => !directConnectionByMethods[c.Method1].Contains(c.Method2))
                    .Where(c => !directConnectionByMethods[c.Method2].Contains(c.Method1));

                connections.AddRange(indirectlyConnectedMethods);
            }

            return new MethodGraph(connections.Distinct(new MethodConnectionComparer()), reverseIndex.VisibleMethods);
        }

        private static IEnumerable<MethodConnection> ConnectAllMethods(IReadOnlyCollection<IMethodSymbol> methods, MethodConnection.ConnectionType connectionType)
        {
            // We do a cartesian product of all the methods in the group, and remove self connections
            return methods
                .SelectMany(x => methods, (x, y) => new MethodConnection(connectionType, x, y))
                .Where(connection => !connection.IsSelfConnected);
        }
    }
}
