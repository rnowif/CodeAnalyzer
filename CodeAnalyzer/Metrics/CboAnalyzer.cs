using System.Linq;
using CodeAnalyzer.Tree;

namespace CodeAnalyzer.Metrics
{
    /// <summary>
    /// Coupling between objects (CBO) = sum(dependencies) / count(classes)
    /// For each class, count the number of dependencies and sum them
    /// Could do the same for indirect dependencies
    /// There is a relationship between the CBO and the number of defects in a codebase.
    /// </summary>
    public static class CboAnalyzer
    {
        public static float ComputeCouplingBetweenObjects(this SourceAnalyzer analyzer)
        {
            var numberOfDependencies = 0;
            analyzer.VisitClasses(classAnalyzer =>
            {
                numberOfDependencies += classAnalyzer.FindDependencies().Count();
            });

            return numberOfDependencies / (float) analyzer.ClassCount;
        }
    }
}
