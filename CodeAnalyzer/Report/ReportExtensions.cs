using System;
using System.Collections.Generic;
using System.Linq;
using CodeAnalyzer.Analysis;
using CodeAnalyzer.Analysis.Cohesion;
using CodeAnalyzer.Analysis.Coupling;

namespace CodeAnalyzer.Report
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

                var methodAnalyzer = ClassCohesionAnalyzer.FromClass(node.Class);

                classReports[node.Identifier] = new ClassAnalysisReport(
                    node.Identifier,
                    node.Dependencies.Count(),
                    methodAnalyzer.ComputeTightClassCohesion(),
                    methodAnalyzer.ComputeLooseClassCohesion(),
                    methodAnalyzer.ComputeLackOfCohesionOfMethod()
                );
            }

            return new AnalysisReport(analyzer.DependencyGraph.ComputeCouplingBetweenObjects(predicate), classReports);
        }
    }
}
