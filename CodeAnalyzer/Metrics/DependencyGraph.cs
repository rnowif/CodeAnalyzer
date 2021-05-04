using System.Collections.Generic;
using System.Linq;
using CodeAnalyzer.Tree;

namespace CodeAnalyzer.Metrics
{
    public class DependencyGraph
    {
        private readonly Dictionary<string, IList<string>> _dependenciesByClassName;
        private readonly Dictionary<string, IList<string>> _referencesByClassName;

        public int Count => Classes.Count();
        public IEnumerable<ClassAnalyzer> Classes { get; }

        private DependencyGraph(IEnumerable<ClassAnalyzer> classes, Dictionary<string, IList<string>> dependenciesByClassName, Dictionary<string, IList<string>> referencesByClassName)
        {
            Classes = classes;
            _dependenciesByClassName = dependenciesByClassName;
            _referencesByClassName = referencesByClassName;
        }

        public IEnumerable<string> FindDependencies(string className) => _dependenciesByClassName[className].ToHashSet();
        public IEnumerable<string> FindReferences(string className) => _referencesByClassName[className].ToHashSet();

        public static DependencyGraph FromClasses(IReadOnlyCollection<ClassAnalyzer> classes)
        {
            Dictionary<string, IList<string>> dependenciesByClassName = new Dictionary<string, IList<string>>();
            Dictionary<string, IList<string>> referencesByClassName = new Dictionary<string, IList<string>>();

            foreach (var @class in classes)
            {
                var dependencies = @class.FindDependencies().ToList();
                dependenciesByClassName[@class.QualifiedName] = dependencies;

                foreach (var dependency in dependencies)
                {
                    if (!referencesByClassName.ContainsKey(dependency))
                    {
                        referencesByClassName[dependency] = new List<string>();
                    }

                    referencesByClassName[dependency].Add(@class.QualifiedName);
                }
            }

            return new DependencyGraph(classes, dependenciesByClassName, referencesByClassName);
        }
    }
}
