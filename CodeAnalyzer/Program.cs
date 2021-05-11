using System;
using System.Linq;
using CodeAnalyzer.Analyzer;
using CodeAnalyzer.Dependencies;
using CodeAnalyzer.Methods;
using CodeAnalyzer.Metrics;

namespace CodeAnalyzer
{
    class Program
    {
        private const string SourcesDir = @"C:\WF\LP\server";

        public static void Main(string[] args)
        {
            // See: https://www.researchgate.net/publication/2540411_Thresholds_for_Object-Oriented_Measures for thresholds
            var analyzer = SourceAnalyzer.FromDirectory(SourcesDir);

            var nodesExcludingPortals = analyzer.DependencyGraph.Nodes
                .Where(n => !n.Identifier.EndsWith("Portal"))
                .ToList();

            (DependencyNode, float) worstTccOffender = (null, 0)!;
            (DependencyNode, int) worstDependenciesOffender = (null, 0)!;

            foreach (var node in nodesExcludingPortals)
            {
                Console.WriteLine($"Analyzing {node.Identifier}...");

                // Dependencies
                var dependencyCount = node.Dependencies.Count();
                if (worstDependenciesOffender.Item1 == null || dependencyCount > worstDependenciesOffender.Item2)
                {
                    worstDependenciesOffender = (node, dependencyCount);
                }

                var graph = MethodGraph.FromClass(node.Class);

                // TCC
                var tcc = graph.ComputeTightClassCohesion();
                if (tcc.HasValue && (worstTccOffender.Item1 == null || tcc < worstTccOffender.Item2))
                {
                    worstTccOffender = (node, tcc.Value);
                }
            }

            Console.WriteLine($"Analysed {analyzer.DependencyGraph.Count} classes.");
            Console.WriteLine($"Coupling Between Objects (CBO): {analyzer.DependencyGraph.ComputeCouplingBetweenObjects()}");
            Console.WriteLine($"Worst Offender: {worstDependenciesOffender.Item1.Identifier} has {worstDependenciesOffender.Item2} dependencies");
            Console.WriteLine($"Worst Offender: {worstTccOffender.Item1.Identifier} has a TCC of {worstTccOffender.Item2}");
        }
    }
}
