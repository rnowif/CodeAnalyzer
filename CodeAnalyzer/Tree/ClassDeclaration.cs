using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalyzer.Tree
{
    public class ClassDeclaration
    {
        private readonly string _namespaceName;
        private readonly string _name;

        public ClassDeclarationSyntax Syntax { get; }
        public string QualifiedName => $"{_namespaceName}.{_name}";

        public ClassDeclaration(ClassDeclarationSyntax classDeclarationSyntax, string namespaceName)
        {
            _name = classDeclarationSyntax.Identifier.ToString();
            _namespaceName = namespaceName;
            Syntax = classDeclarationSyntax;
        }
    }
}
