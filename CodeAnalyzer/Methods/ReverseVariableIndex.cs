﻿using System.Collections.Generic;
using System.Linq;
using CodeAnalyzer.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalyzer.Methods
{
    public class ReverseVariableIndex
    {
        private readonly Dictionary<ISymbol, List<IMethodSymbol>> _variablesReverseIndex;
        private readonly List<IMethodSymbol> _visibleMethods;

        private ReverseVariableIndex(Dictionary<ISymbol, List<IMethodSymbol>> variablesReverseIndex, List<IMethodSymbol> visibleMethods)
        {
            _variablesReverseIndex = variablesReverseIndex;
            _visibleMethods = visibleMethods;
        }

        public IReadOnlyDictionary<ISymbol, List<IMethodSymbol>> MethodGroups => _variablesReverseIndex;
        public IEnumerable<IMethodSymbol> VisibleMethods => _visibleMethods;

        public static ReverseVariableIndex Build(ClassAnalyzer @class)
        {
            var variablesReverseIndex = new Dictionary<ISymbol, List<IMethodSymbol>>();

            // Building a reverse-index of variables accessed by methods
            var classLevelVariables = @class.GetClassLevelVariables();

            var classMethods = new List<IMethodSymbol>();
            var variablesByMethods = new Dictionary<IMethodSymbol, IEnumerable<ISymbol>>();
            var methodCallGraph = new MethodGraph();
            foreach (var method in @class.Syntax.Members.OfType<MethodDeclarationSyntax>())
            {
                var symbol = @class.SemanticModel.GetDeclaredSymbol(method) as IMethodSymbol;
                classMethods.Add(symbol!);

                variablesByMethods[symbol] = method.DescendantNodes().OfType<IdentifierNameSyntax>()
                    .Select(s => @class.SemanticModel.GetSymbolInfo(s).Symbol)
                    .Where(s => s != null && classLevelVariables.Any(s.Equals))
                    .Select(s => s!);

                var methodsCalled = method.DescendantNodes().OfType<IdentifierNameSyntax>()
                    .Select(s => TryGetMethodSymbol(@class, s))
                    .Where(s => s != null && classMethods.Any(s.Equals))
                    .Select(s => s!);

                methodCallGraph.AddChildren(symbol, methodsCalled);
            }

            var visibleMethods = new List<IMethodSymbol>();
            foreach (var method in classMethods.Where(IsVisible))
            {
                visibleMethods.Add(method);
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

            return new ReverseVariableIndex(variablesReverseIndex, visibleMethods);
        }

        private static IMethodSymbol? TryGetMethodSymbol(ClassAnalyzer @class, SyntaxNode node)
        {
            var symbolInfo = @class.SemanticModel.GetSymbolInfo(node);

            // The symbol might not be found for multiple reasons, so we try and find a suitable candidate
            return (symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault()) as IMethodSymbol;
        }

        private static bool IsVisible(IMethodSymbol method) => method.DeclaredAccessibility != Accessibility.Private && !method.IsStatic && !method.IsOverride;
    }
}
