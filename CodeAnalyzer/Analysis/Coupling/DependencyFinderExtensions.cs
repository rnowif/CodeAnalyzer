using System.Collections.Generic;
using System.Linq;

namespace CodeAnalyzer.Analysis.Coupling;

public static class DependencyFinderExtensions
{
    public static IEnumerable<string> FindDependencies(this ClassAnalyzer classAnalyzer)
    {
        return new DependencyFinder(classAnalyzer)
            .FindDependencies()
            .Select(t => t?.ToString() ?? "");
    }

}
