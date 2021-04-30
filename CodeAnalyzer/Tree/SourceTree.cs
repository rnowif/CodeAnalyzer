using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalyzer.Tree
{
    public class SourceTree
    {
        private readonly Dictionary<string, ClassDeclaration> _classesByName;

        private SourceTree(Dictionary<string, ClassDeclaration> classesByName)
        {
            _classesByName = classesByName;
        }

        public static SourceTree FromSources(string rootDir)
        {
            var rootDirInfo = new DirectoryInfo(rootDir);
            var classPaths = rootDirInfo
                .GetFiles("*.cs", SearchOption.AllDirectories)
                .Select(fileInfo => Path.Join((string?) fileInfo.DirectoryName, (string?) fileInfo.Name));

            var classesByName = new Dictionary<string, ClassDeclaration>();

            foreach (var sourcePath in classPaths)
            {
                var content = File.ReadAllText(sourcePath);
                var syntaxTree = CSharpSyntaxTree.ParseText(content);
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

            return new SourceTree(classesByName);
        }
    }
}
