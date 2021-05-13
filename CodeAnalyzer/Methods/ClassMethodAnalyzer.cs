using System.Collections.Generic;
using System.Linq;
using CodeAnalyzer.Analyzer;
using Microsoft.CodeAnalysis;

namespace CodeAnalyzer.Methods
{
    public class ClassMethodAnalyzer
    {
        private readonly IReadOnlyCollection<MethodConnection> _connections;
        private readonly IReadOnlyCollection<IMethodSymbol> _methods;

        public int CountVisible => _methods.Count;
        public int CountDirectConnections => _connections.Count(c => c.Type == MethodConnection.ConnectionType.Direct);
        public int CountIndirectConnections => _connections.Count(c => c.Type == MethodConnection.ConnectionType.Indirect);

        private ClassMethodAnalyzer(IReadOnlyDictionary<IMethodSymbol, HashSet<IMethodSymbol>> directConnectionGraph, IEnumerable<IMethodSymbol> methods)
        {
            _methods = methods.ToList();
            _connections = BuildConnections(directConnectionGraph);
        }

        private static IReadOnlyCollection<MethodConnection> BuildConnections(IReadOnlyDictionary<IMethodSymbol, HashSet<IMethodSymbol>> directConnectionGraph)
        {
            var connections = new List<MethodConnection>();

            // For each key, all the associated methods are directly connected to the key and indirectly connected to each other
            foreach (var (method, connectedMethods) in directConnectionGraph)
            {
                var directConnections = connectedMethods
                    .Select(m => new MethodConnection(MethodConnection.ConnectionType.Direct, method, m));

                connections.AddRange(directConnections);

                var indirectConnections = ConnectAllMethods(connectedMethods, MethodConnection.ConnectionType.Indirect)
                    .Where(c => !directConnectionGraph[c.Method1].Contains(c.Method2))
                    .Where(c => !directConnectionGraph[c.Method2].Contains(c.Method1));

                connections.AddRange(indirectConnections);
            }

            return connections.Distinct(new MethodConnectionComparer()).ToList();
        }

        private static IEnumerable<MethodConnection> ConnectAllMethods(IReadOnlyCollection<IMethodSymbol> methods, MethodConnection.ConnectionType connectionType)
        {
            // We do a cartesian product of all the methods in the group, and remove self connections
            return methods
                .SelectMany(x => methods, (x, y) => new MethodConnection(connectionType, x, y))
                .Where(connection => !connection.IsSelfConnected);
        }

        public static ClassMethodAnalyzer FromClass(ClassAnalyzer @class)
        {
            // Marking all methods that share a variable as directly connected
            // Methods A and B are directly connected if:
            // - They both access the same field/property, or
            // - The call trees starting at A and B access the same field/property.

            // Methods A and B are indirectly connected if:
            // - They share a direct connection but are not directly connected themselves

            var reverseIndex = ReverseVariableIndex.Build(@class);
            var directConnectionGraph = new Dictionary<IMethodSymbol, HashSet<IMethodSymbol>>();

            // All methods accessing the same variables are directly connected to each other
            foreach (var (_, methods) in reverseIndex.MethodGroups)
            {
                // We do a cartesian product to mark all these methods are directly connected to each other
                foreach (var method in methods)
                {
                    if (!directConnectionGraph.ContainsKey(method))
                    {
                        directConnectionGraph[method] = new HashSet<IMethodSymbol>();
                    }

                    // A method cannot be connected to itself
                    foreach (var m in methods.Where(m => m != method))
                    {
                        directConnectionGraph[method].Add(m);
                    }
                }
            }

            return new ClassMethodAnalyzer(directConnectionGraph, reverseIndex.VisibleMethods);
        }
    }
}
