using System;
using System.Collections.Generic;
using System.Linq;
using CodeAnalyzer.Analyzer;
using CodeAnalyzer.Dependencies;
using CodeAnalyzer.Methods;

namespace CodeAnalyzer.Metrics
{
    public static class SourceAnalyzerExtensions
    {
        public static AnalysisReport Analyze(this SourceAnalyzer analyzer, Predicate<DependencyNode>? selectNodes = null)
        {
            Predicate<DependencyNode> predicate = selectNodes ?? (node => true);

            Dictionary<string, ClassAnalysisReport> classReports = new Dictionary<string, ClassAnalysisReport>();

            foreach (var node in analyzer.DependencyGraph.Nodes.Where(node => predicate(node)))
            {
                Console.WriteLine($"Analyzing {node.Identifier}...");

                var graph = MethodGraph.FromClass(node.Class);

                classReports[node.Identifier] = new ClassAnalysisReport(
                    node.Identifier,
                    node.Dependencies.Count(),
                    graph.ComputeTightClassCohesion(),
                    graph.ComputeLooseClassCohesion()
                );
            }

            return new AnalysisReport(analyzer.DependencyGraph.ComputeCouplingBetweenObjects(predicate), classReports);
        }
    }
}
