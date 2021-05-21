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
        public static AnalysisReport Analyze(this SourceAnalyzer analyzer, AnalysisConfiguration configuration)
        {
            Dictionary<string, ClassAnalysisReport> classReports = new Dictionary<string, ClassAnalysisReport>();

            foreach (var node in analyzer.DependencyGraph.Nodes.Where(configuration.SelectNode))
            {
                Console.WriteLine($"Analyzing {node.Identifier}...");

                var cohesionAnalyzer = ClassCohesionAnalyzer.FromClass(node.Class, configuration);

                classReports[node.Identifier] = new ClassAnalysisReport(
                    node.Identifier,
                    node.Dependencies.Count(),
                    cohesionAnalyzer.ComputeTightClassCohesion(),
                    cohesionAnalyzer.ComputeLooseClassCohesion(),
                    cohesionAnalyzer.GetMethodGroups().Select(g => g.Select(m => m.Name))
                );
            }

            return new AnalysisReport(analyzer.DependencyGraph.ComputeCouplingBetweenObjects(configuration), classReports);
        }
    }
}
