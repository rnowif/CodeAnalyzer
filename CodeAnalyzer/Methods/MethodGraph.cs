using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CodeAnalyzer.Methods
{
    public class MethodGraph
    {
        private readonly Dictionary<IMethodSymbol, HashSet<IMethodSymbol>> _internalGraph = new Dictionary<IMethodSymbol, HashSet<IMethodSymbol>>();
        public IEnumerable<MethodGraphNode> Nodes => _internalGraph.Select(p => new MethodGraphNode(p.Key, p.Value));

        public void AddConnections(IMethodSymbol sourceNode, IEnumerable<IMethodSymbol> targetNodes)
        {
            if (!_internalGraph.ContainsKey(sourceNode))
            {
                _internalGraph[sourceNode] = new HashSet<IMethodSymbol>();
            }

            // A method cannot be connected to itself
            foreach (var m in targetNodes.Where(m => !m.Equals(sourceNode, SymbolEqualityComparer.Default)))
            {
                _internalGraph[sourceNode].Add(m);
            }
        }

        public bool AreConnected(IMethodSymbol method1, IMethodSymbol method2) =>
            _internalGraph[method1].Contains(method2) || _internalGraph[method2].Contains(method1);

        public IEnumerable<IMethodSymbol> GetCallTree(IMethodSymbol method) => GetCallTree(method, new List<IMethodSymbol>());

        private IEnumerable<IMethodSymbol> GetCallTree(IMethodSymbol method, IReadOnlyCollection<IMethodSymbol> breadcrumb)
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
                subMethods.AddRange(GetCallTree(subMethod, updatedBreadcrumb));
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
