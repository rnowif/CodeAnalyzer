using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CodeAnalyzer.Analysis.Cohesion
{
    public class ClassCohesionAnalyzer
    {
        private readonly ClassIndex _classIndex;
        private readonly IReadOnlyCollection<MethodConnection> _connections;

        public int CountVisible => _classIndex.VisibleMethods.Count();
        public int CountDirectConnections => _connections.Count(c => c.Type == MethodConnection.ConnectionType.Direct);
        public int CountIndirectConnections => _connections.Count(c => c.Type == MethodConnection.ConnectionType.Indirect);

        private ClassCohesionAnalyzer(ClassIndex classIndex)
        {
            _classIndex = classIndex;

            var directConnectionGraph = new MethodGraph();

            // All methods accessing the same variables are directly connected to each other
            foreach (var (_, methods) in classIndex.ReverseVariableIndex)
            {
                // We do a cartesian product to mark all these methods are directly connected to each other
                foreach (var method in methods)
                {
                    directConnectionGraph.AddChildren(method, methods);
                }
            }

            _connections = BuildConnections(directConnectionGraph);
        }

        private static IReadOnlyCollection<MethodConnection> BuildConnections(MethodGraph directConnectionGraph)
        {
            var connections = new List<MethodConnection>();

            // For each key, all the associated methods are directly connected to the key and indirectly connected to each other
            foreach (var node in directConnectionGraph.Nodes)
            {
                var directConnections = node.TargetNodes
                    .Select(m => new MethodConnection(MethodConnection.ConnectionType.Direct, node.SourceNode, m));

                connections.AddRange(directConnections);

                var indirectConnections = ConnectAllMethods(node.TargetNodes.ToList(), MethodConnection.ConnectionType.Indirect)
                    .Where(c => !directConnectionGraph.AreConnected(c.Method1, c.Method2));

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

        public static ClassCohesionAnalyzer FromClass(ClassAnalyzer @class) => new ClassCohesionAnalyzer(ClassIndex.Build(@class));
    }
}
