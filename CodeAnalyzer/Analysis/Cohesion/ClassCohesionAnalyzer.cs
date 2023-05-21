using System.Collections.Generic;
using System.Linq;
using CodeAnalyzer.Report;
using Microsoft.CodeAnalysis;

namespace CodeAnalyzer.Analysis.Cohesion;

public class ClassCohesionAnalyzer
{
    private readonly ClassIndex _classIndex;
    private readonly IReadOnlyCollection<MethodConnection> _connections;
    private readonly MethodGraph _directConnectionGraph;

    public int CountVisible => _classIndex.VisibleMethods.Count();
    public int CountDirectConnections => _connections.Count(c => c.Type == MethodConnection.ConnectionType.Direct);
    public int CountIndirectConnections => _connections.Count(c => c.Type == MethodConnection.ConnectionType.Indirect);

    private ClassCohesionAnalyzer(ClassIndex classIndex)
    {
        _classIndex = classIndex;
        _directConnectionGraph = new MethodGraph();

        // All methods accessing the same variables are directly connected to each other
        foreach (var (_, methods) in classIndex.ReverseVariableIndex)
        {
            // We do a cartesian product to mark all these methods are directly connected to each other
            foreach (var method in methods)
            {
                _directConnectionGraph.AddChildren(method, methods);
            }
        }

        _connections = BuildConnections(_directConnectionGraph);
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
            .SelectMany(_ => methods, (x, y) => new MethodConnection(connectionType, x, y))
            .Where(connection => !connection.IsSelfConnected);
    }

    internal IEnumerable<IEnumerable<IMethodSymbol>> GetMethodGroups()
    {
        // Methods a and b are related if:
        // - they are directly connected, or
        //  - a calls b, or b calls a.

        // Therefore, we create a "relation graph" that is the combination of the direct connection graph and the call graph
        // to extract the connected components

        var combinedGraph = new MethodGraph();

        foreach (var node in _directConnectionGraph.Nodes)
        {
            combinedGraph.AddChildren(node.SourceNode, node.TargetNodes);
        }

        foreach (var node in _classIndex.CallGraph.Nodes.Where(n => !IsStaticOrOverride(n.SourceNode)))
        {
            combinedGraph.AddChildren(node.SourceNode, node.TargetNodes);
        }

        return combinedGraph.GetConnectedComponents();
    }
    
    private static bool IsStaticOrOverride(IMethodSymbol method) => method.IsStatic || method.IsOverride || method.IsInterfaceImplementation();

    public static ClassCohesionAnalyzer FromClass(ClassAnalyzer @class, AnalysisConfiguration configuration) => new(ClassIndex.Build(@class, configuration));
}
