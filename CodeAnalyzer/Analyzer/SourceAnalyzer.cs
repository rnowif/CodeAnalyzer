using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeAnalyzer.Dependencies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalyzer.Analyzer
{
    public class SourceAnalyzer
    {
        public DependencyGraph DependencyGraph { get; }

        private SourceAnalyzer(IEnumerable<Compilation> compilations)
        {
            var classes = new List<ClassAnalyzer>();

            foreach (var compilation in compilations)
            {
                foreach (var syntaxTree in compilation.SyntaxTrees)
                {
                    var root = syntaxTree.GetCompilationUnitRoot();

                    foreach (var namespaceSyntax in root.Members.OfType<NamespaceDeclarationSyntax>())
                    {
                        var namespaceName = namespaceSyntax.Name.ToString();
                        classes.AddRange(namespaceSyntax.Members.OfType<ClassDeclarationSyntax>()
                            .Select(classSyntax => new ClassAnalyzer(classSyntax, namespaceName, compilation.GetSemanticModel(classSyntax.SyntaxTree))));
                    }
                }
            }

            DependencyGraph = DependencyGraph.FromClasses(classes);
        }

        public static SourceAnalyzer FromDirectory(string directory)
        {
            var classFiles = new DirectoryInfo(directory)
                .GetFiles("*.cs", SearchOption.AllDirectories)
                .Select(fileInfo => Path.Join(fileInfo.DirectoryName, fileInfo.Name));

            return FromFiles(classFiles);
        }

        public static SourceAnalyzer FromFile(string filePath) => FromFiles(new[] {filePath});

        private static SourceAnalyzer FromFiles(IEnumerable<string> classPaths)
        {
            var syntaxTrees = classPaths
                .Select(File.ReadAllText)
                .Select(content => CSharpSyntaxTree.ParseText(content))
                .ToList();

            return new SourceAnalyzer(new[] {CSharpCompilation.Create(null, syntaxTrees)});
        }
    }
}
