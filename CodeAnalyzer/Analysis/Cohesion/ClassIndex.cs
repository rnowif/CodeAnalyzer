using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalyzer.Analysis.Cohesion
{
    public class ClassIndex
    {
        private readonly Dictionary<ISymbol, List<IMethodSymbol>> _reverseVariableIndex;

        private ClassIndex(Dictionary<ISymbol, List<IMethodSymbol>> reverseVariableIndex, MethodGraph callGraph)
        {
            _reverseVariableIndex = reverseVariableIndex;

            CallGraph = callGraph;
            VisibleMethods = callGraph.Nodes.Select(n => n.SourceNode).Where(IsVisible);
        }

        public IReadOnlyDictionary<ISymbol, List<IMethodSymbol>> ReverseVariableIndex => _reverseVariableIndex;
        public IEnumerable<IMethodSymbol> VisibleMethods { get; }

        public MethodGraph CallGraph { get; }

        private static bool IsVisible(IMethodSymbol method) => method.DeclaredAccessibility != Accessibility.Private && !method.IsStatic && !method.IsOverride;

        public static ClassIndex Build(ClassAnalyzer @class)
        {
            var variablesReverseIndex = new Dictionary<ISymbol, List<IMethodSymbol>>();

            // Building a reverse-index of variables accessed by methods
            var classLevelVariables = @class.GetClassLevelVariables();

            var variablesByMethods = new Dictionary<IMethodSymbol, IEnumerable<ISymbol>>();
            var methodCallGraph = new MethodGraph();
            foreach (var method in @class.Syntax.Members.OfType<MethodDeclarationSyntax>())
            {
                var symbol = @class.SemanticModel.GetDeclaredSymbol(method) as IMethodSymbol;

                variablesByMethods[symbol!] = method.DescendantNodes().OfType<IdentifierNameSyntax>()
                    .Select(s => @class.SemanticModel.GetSymbolInfo(s).Symbol)
                    .Where(s => s != null && classLevelVariables.Any(s.Equals))
                    .Select(s => s!);

                var methodsCalled = method.DescendantNodes().OfType<IdentifierNameSyntax>()
                    .Select(s => TryGetMethodSymbol(@class, s))
                    .Where(s => s != null)
                    .Select(s => s!);

                methodCallGraph.AddChildren(symbol, methodsCalled);
            }

            foreach (var method in methodCallGraph.Nodes.Select(n => n.SourceNode).Where(IsVisible))
            {
                var methodVariables = variablesByMethods[method].ToList();

                foreach (var subMethod in methodCallGraph.GetRecursiveChildren(method))
                {
                    methodVariables.AddRange(variablesByMethods[subMethod]);
                }

                foreach (var variable in methodVariables)
                {
                    if (!variablesReverseIndex.ContainsKey(variable))
                    {
                        variablesReverseIndex[variable] = new List<IMethodSymbol>();
                    }

                    variablesReverseIndex[variable].Add(method);
                }
            }

            return new ClassIndex(variablesReverseIndex, methodCallGraph);
        }

        private static IMethodSymbol? TryGetMethodSymbol(ClassAnalyzer @class, SyntaxNode node)
        {
            var symbolInfo = @class.SemanticModel.GetSymbolInfo(node);

            // The symbol might not be found for multiple reasons, so we try and find a suitable candidate
            return (symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault()) as IMethodSymbol;
        }
    }
}
