using System;
using System.Linq;
using System.Threading.Tasks;
using CodeAnalyzer.Analysis;
using CodeAnalyzer.Analysis.Coupling;
using CodeAnalyzer.Report;

namespace CodeAnalyzer;

public abstract class Program
{
    public static async Task Main(string[] args)
    {
        // See: https://www.researchgate.net/publication/2540411_Thresholds_for_Object-Oriented_Measures for thresholds
        var analyzer = await SourceAnalyzer.FromSolution(args[0]);

        var configuration = AnalysisConfiguration.New()
            .WhereNodes(node => NotATest(node) && NotAStartupClass(node))
            .Build();

        var report = analyzer.Analyze(configuration);

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

        var worstLCom4Offenders = report.ClassesReports
            .Select(r => r.Value)
            .OrderByDescending(r => r.LackOfCohesionOfMethods)
            .Take(5);

        Console.WriteLine("Top 5 worst Lack of Cohesion of Methods (LCOM4) offenders");
        foreach (var offender in worstLCom4Offenders)
        {
            Console.WriteLine($"\t- {offender.Identifier} has a LCOM4 of {offender.LackOfCohesionOfMethods}");
        }
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
