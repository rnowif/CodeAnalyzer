using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalyzer.Analyzer
{
    public class ClassAnalyzer
    {
        private readonly string _namespaceName;
        private readonly string _name;

        public ClassDeclarationSyntax Syntax { get; }
        public SemanticModel SemanticModel { get; }
        public string QualifiedName => $"{_namespaceName}.{_name}";

        public ClassAnalyzer(ClassDeclarationSyntax syntax, string namespaceName, SemanticModel semanticModel)
        {
            _name = syntax.Identifier.ToString();
            _namespaceName = namespaceName;
            Syntax = syntax;
            SemanticModel = semanticModel;
        }
    }
}
