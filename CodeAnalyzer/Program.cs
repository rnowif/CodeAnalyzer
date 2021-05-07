using System;
using System.Linq;
using CodeAnalyzer.Analyzer;
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

            var worstOffender = analyzer.DependencyGraph.Nodes
                .Aggregate((curMax, x) => x.Dependencies.Count() > curMax.Dependencies.Count() ? x : curMax);
            Console.WriteLine($"Worst Offender: {worstOffender.Identifier} has {worstOffender.Dependencies.Count()} dependencies");

            var worstOffenderExcludingPortals = analyzer.DependencyGraph.Nodes
                .Where(n => !n.Identifier.EndsWith("Portal"))
                .Aggregate((curMax, x) => x.Dependencies.Count() > curMax.Dependencies.Count() ? x : curMax);
            Console.WriteLine($"Worst Offender (excluding portals): {worstOffenderExcludingPortals.Identifier} has {worstOffenderExcludingPortals.Dependencies.Count()} dependencies");
        }
    }
}
