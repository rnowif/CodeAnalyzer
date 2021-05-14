using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CodeAnalyzer.Methods
{
    public class MethodGraph
    {
        private readonly Dictionary<IMethodSymbol, HashSet<IMethodSymbol>> _internalGraph = new Dictionary<IMethodSymbol, HashSet<IMethodSymbol>>();
        public IEnumerable<MethodGraphNode> Nodes => _internalGraph.Select(p => new MethodGraphNode(p.Key, p.Value));

        public void AddChildren(IMethodSymbol node, IEnumerable<IMethodSymbol> children)
        {
            if (!_internalGraph.ContainsKey(node))
            {
                _internalGraph[node] = new HashSet<IMethodSymbol>();
            }

            // A method cannot be connected to itself
            foreach (var m in children.Where(m => !m.Equals(node, SymbolEqualityComparer.Default)))
            {
                _internalGraph[node].Add(m);
            }
        }

        /// <summary>
        /// Returns whether a method is a direct child of another method (or the other way around)
        /// </summary>
        /// <param name="method1"></param>
        /// <param name="method2"></param>
        /// <returns>True if method1 is a directly child of method2 or vice-versa</returns>
        public bool AreConnected(IMethodSymbol method1, IMethodSymbol method2) =>
            _internalGraph[method1].Contains(method2) || _internalGraph[method2].Contains(method1);

        public IEnumerable<IMethodSymbol> GetRecursiveChildren(IMethodSymbol method) => GetRecursiveChildren(method, new List<IMethodSymbol>());

        private IEnumerable<IMethodSymbol> GetRecursiveChildren(IMethodSymbol method, IReadOnlyCollection<IMethodSymbol> breadcrumb)
        {
            // Early exit to handle methods recursively calling themselves
            if (breadcrumb.Contains(method))
            {
                return Enumerable.Empty<IMethodSymbol>();
            }

            var updatedBreadcrumb = breadcrumb.Concat(new[] {method}).ToList();

            var subMethods = new List<IMethodSymbol>();
            foreach (var subMethod in _internalGraph[method])
            {
                subMethods.Add(subMethod);
                subMethods.AddRange(GetRecursiveChildren(subMethod, updatedBreadcrumb));
            }

            return subMethods;
        }
    }

    public class MethodGraphNode
    {
        public IMethodSymbol SourceNode { get; }
        public IEnumerable<IMethodSymbol> TargetNodes { get; }

        public MethodGraphNode(IMethodSymbol sourceNode, IEnumerable<IMethodSymbol> targetNodes)
        {
            SourceNode = sourceNode;
            TargetNodes = targetNodes;
        }
    }
}
