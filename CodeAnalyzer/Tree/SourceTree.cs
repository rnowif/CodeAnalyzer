using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalyzer.Tree
{
    public class SourceTree
    {
        private readonly Dictionary<string, ClassDeclaration> _classesByName;
        private readonly CSharpCompilation _compilation;

        public IReadOnlyCollection<ClassDeclaration> Classes => _classesByName.Values;

        private SourceTree(Dictionary<string, ClassDeclaration> classesByName, CSharpCompilation compilation)
        {
            _classesByName = classesByName;
            _compilation = compilation;
        }

        public SemanticModel FindModel(ClassDeclaration declaration) => _compilation.GetSemanticModel(declaration.Syntax.SyntaxTree);

        public static SourceTree FromDirectory(string directory)
        {
            var classFiles = new DirectoryInfo(directory)
                .GetFiles("*.cs", SearchOption.AllDirectories)
                .Select(fileInfo => Path.Join(fileInfo.DirectoryName, fileInfo.Name));

            return FromFiles(classFiles);
        }

        public static SourceTree FromFile(string filePath) => FromFiles(new[] {filePath});

        public static SourceTree FromFiles(IEnumerable<string> classPaths)
        {
            var syntaxTrees = new List<SyntaxTree>();
            var classesByName = new Dictionary<string, ClassDeclaration>();

            foreach (var sourcePath in classPaths)
            {
                var content = File.ReadAllText(sourcePath);
                var syntaxTree = CSharpSyntaxTree.ParseText(content);
                syntaxTrees.Add(syntaxTree);
                var root = syntaxTree.GetCompilationUnitRoot();

                foreach (var namespaceSyntax in root.Members.OfType<NamespaceDeclarationSyntax>())
                {
                    var namespaceName = namespaceSyntax.Name.ToString();
                    foreach (var classSyntax in namespaceSyntax.Members.OfType<ClassDeclarationSyntax>())
                    {
                        var cSharpClass = new ClassDeclaration(classSyntax, namespaceName);
                        classesByName[cSharpClass.QualifiedName] = cSharpClass;
                    }
                }
            }

            var compilation = CSharpCompilation.Create(null, syntaxTrees);
            return new SourceTree(classesByName, compilation);
        }
    }
}
