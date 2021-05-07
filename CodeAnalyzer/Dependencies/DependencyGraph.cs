using System.Collections.Generic;
using System.Linq;
using CodeAnalyzer.Analyzer;

namespace CodeAnalyzer.Dependencies
{
    public class DependencyGraph
    {
        private readonly Dictionary<string, DependencyNode> _nodesByClassName;

        public int Count => _nodesByClassName.Count;
        public IEnumerable<DependencyNode> Nodes => _nodesByClassName.Values;

        private DependencyGraph(Dictionary<string,DependencyNode> nodesByClassName)
        {
            _nodesByClassName = nodesByClassName;
        }

        public IEnumerable<string> FindDependencies(string className) =>
            _nodesByClassName.TryGetValue(className, out var node) ? node.Dependencies : Enumerable.Empty<string>();

        public IEnumerable<string> FindReferences(string className) =>
            _nodesByClassName.TryGetValue(className, out var node) ? node.References : Enumerable.Empty<string>();

        public static DependencyGraph FromClasses(IReadOnlyCollection<ClassAnalyzer> classes)
        {
            Dictionary<string, IList<string>> dependenciesByClassName = new Dictionary<string, IList<string>>();
            Dictionary<string, IList<string>> referencesByClassName = new Dictionary<string, IList<string>>();

            Dictionary<string, DependencyNode> nodesByClassName = new Dictionary<string, DependencyNode>();

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

            foreach (var @class in classes)
            {
                if (!dependenciesByClassName.TryGetValue(@class.QualifiedName, out var dependencies))
                {
                    dependencies = new List<string>();
                }

                if (!referencesByClassName.TryGetValue(@class.QualifiedName, out var references))
                {
                    references = new List<string>();
                }

                nodesByClassName[@class.QualifiedName] = new DependencyNode(@class.QualifiedName, dependencies.ToHashSet(), references.ToHashSet());
            }

            return new DependencyGraph(nodesByClassName);
        }
    }

    public class DependencyNode
    {
        public string Identifier { get; }
        public IEnumerable<string> Dependencies { get; }
        public IEnumerable<string> References { get; }

        public DependencyNode(string className, IEnumerable<string> dependencies, IEnumerable<string> references)
        {
            Identifier = className;
            Dependencies = dependencies;
            References = references;
        }
    }
}
