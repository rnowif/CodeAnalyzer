using System;
using System.Linq;
using CodeAnalyzer.Analyzer;
using CodeAnalyzer.Dependencies;
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
            Console.WriteLine($"Analysed {analyzer.DependencyGraph.Count} classes.");
            Console.WriteLine($"Coupling Between Objects (CBO): {analyzer.DependencyGraph.ComputeCouplingBetweenObjects()}");

            var nodesExcludingPortals = analyzer.DependencyGraph.Nodes
                .Where(n => !n.Identifier.EndsWith("Portal"))
                .ToList();

            var worstOffenderExcludingPortals = nodesExcludingPortals
                .Aggregate((curMax, x) => x.Dependencies.Count() > curMax.Dependencies.Count() ? x : curMax);
            Console.WriteLine($"Worst Offender: {worstOffenderExcludingPortals.Identifier} has {worstOffenderExcludingPortals.Dependencies.Count()} dependencies");

            var dependencyNode = analyzer.DependencyGraph.FindNode("nz.co.LanguagePerfect.Utilities.HtmlToText");
            Console.WriteLine($"{dependencyNode.Identifier} has a TCC of {dependencyNode.Class.ComputeTightClassCohesion()}");

            (DependencyNode, float) worstTccOffender = (null, 0)!;

            foreach (var node in nodesExcludingPortals.Where(n => n.Class.Syntax.SyntaxTree.Length > 1000))
            {
                Console.WriteLine($"Analyzing {node.Identifier}...");
                var tcc = node.Class.ComputeTightClassCohesion();

                if (tcc.HasValue && (worstTccOffender.Item1 == null || tcc < worstTccOffender.Item2))
                {
                    worstTccOffender = (node, tcc.Value);
                    Console.WriteLine($"New worst Offender: {worstTccOffender.Item1.Identifier} has a TCC of {worstTccOffender.Item2}");
                }
            }

            Console.WriteLine($"Worst Offender: {worstTccOffender.Item1.Identifier} has a TCC of {worstTccOffender.Item2}");
        }
    }
}
