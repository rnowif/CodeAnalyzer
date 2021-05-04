using System.Linq;

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
        public static float ComputeCouplingBetweenObjects(this DependencyGraph dependencyGraph)
        {
            var numberOfDependencies = dependencyGraph.Nodes
                .Sum(node => node.Dependencies.Count() + node.References.Count());

            return numberOfDependencies / (float) dependencyGraph.Count;
        }
    }
}
