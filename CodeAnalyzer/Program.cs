using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CodeAnalyzer.Analysis;
using CodeAnalyzer.Analysis.Coupling;
using CodeAnalyzer.Report;
using CodeAnalyzer.Report.Export;

namespace CodeAnalyzer
{
    class Program
    {
        private const string SourcesDir = @"C:\WF\LP\server";

        public static async Task Main(string[] args)
        {
            // See: https://www.researchgate.net/publication/2540411_Thresholds_for_Object-Oriented_Measures for thresholds
            var analyzer = SourceAnalyzer.FromDirectory(SourcesDir);

            var report = analyzer.Analyze(node => NotATest(node) && NotAStartupClass(node));

            Console.WriteLine($"Analysed {report.ClassesReports.Count} classes.");
            Console.WriteLine($"Coupling Between Objects (CBO): {report.CouplingBetweenObjects}");

            var worstDependencyOffenders = report.ClassesReports
                .Select(r => r.Value)
                .OrderByDescending(r => r.DependencyCount)
                .Take(5);

            Console.WriteLine("Top 5 worst Dependency Count offenders");
            foreach (var offender in worstDependencyOffenders)
            {
                Console.WriteLine($"\t- {offender.Identifier} has {offender.DependencyCount} dependencies");
            }

            var worstTccOffenders = report.ClassesReports
                .Select(r => r.Value)
                .OrderBy(r => r.TightClassCohesion)
                .Take(5);

            Console.WriteLine("Top 5 worst Tight Class Cohesion (TCC) offenders");
            foreach (var offender in worstTccOffenders)
            {
                Console.WriteLine($"\t- {offender.Identifier} has a TCC of {offender.TightClassCohesion}");
            }

            var worstLccOffenders = report.ClassesReports
                .Select(r => r.Value)
                .OrderBy(r => r.LooseClassCohesion)
                .Take(5);

            Console.WriteLine("Top 5 worst Loose Class Cohesion (LCC) offenders");
            foreach (var offender in worstLccOffenders)
            {
                Console.WriteLine($"\t- {offender.Identifier} has a LCC of {offender.LooseClassCohesion}");
            }

            Console.WriteLine("Exporting to file...");
            await report.ExportToCsv(Path.Combine(SourcesDir, "analysis_export.csv"));
        }

        private static bool NotATest(DependencyNode node)
        {
            return !node.Identifier.EndsWith("Tests") && !node.Identifier.EndsWith("Test") && !node.Identifier.EndsWith("Fixture");
        }

        private static bool NotAStartupClass(DependencyNode node)
        {
            return !node.Identifier.EndsWith("Startup") && !node.Identifier.EndsWith("Program");
        }
    }
}
