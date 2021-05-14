using System;
using System.Linq;

namespace CodeAnalyzer.Analysis.Coupling
{
    public static class Cbo
    {
        /// <summary>
        /// Coupling between objects (CBO) = sum(dependencies) / count(classes)
        /// For each class, count the number of dependencies and sum them
        /// Could do the same for indirect dependencies
        /// There is a relationship between the CBO and the number of defects in a codebase.
        /// </summary>
        public static float ComputeCouplingBetweenObjects(this DependencyGraph dependencyGraph, Predicate<DependencyNode> selectNodes)
        {
            var numberOfDependencies = dependencyGraph.Nodes
                .Where(node => selectNodes(node))
                .Sum(node => node.Dependencies.Count() + node.References.Count());

            return numberOfDependencies / (float) dependencyGraph.Count;
        }
    }
}
