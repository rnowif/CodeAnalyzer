using CodeAnalyzer.Methods;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalyzer.Analyzer
{
    public class ClassAnalyzer
    {
        public ClassDeclarationSyntax Syntax { get; }
        public SemanticModel SemanticModel { get; }
        public string QualifiedName { get; }
        public MethodGraph MethodGraph { get; }

        public ClassAnalyzer(ClassDeclarationSyntax syntax, string namespaceName, SemanticModel semanticModel)
        {
            QualifiedName = $"{namespaceName}.{syntax.Identifier}";
            Syntax = syntax;
            SemanticModel = semanticModel;

            MethodGraph = MethodGraph.FromClass(this);
        }
    }
}
