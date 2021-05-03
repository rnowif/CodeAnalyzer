using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalyzer.Tree
{
    public class SourceAnalyzer
    {
        private readonly IList<ClassAnalyzer> _classes;

        public int ClassCount => _classes.Count;

        private SourceAnalyzer(Compilation compilation, IEnumerable<SyntaxTree> syntaxTrees)
        {
            _classes = new List<ClassAnalyzer>();

            foreach (var syntaxTree in syntaxTrees)
            {
                var root = syntaxTree.GetCompilationUnitRoot();

                foreach (var namespaceSyntax in root.Members.OfType<NamespaceDeclarationSyntax>())
                {
                    var namespaceName = namespaceSyntax.Name.ToString();
                    foreach (var classSyntax in namespaceSyntax.Members.OfType<ClassDeclarationSyntax>())
                    {
                        var semanticModel = compilation.GetSemanticModel(classSyntax.SyntaxTree);
                        var cSharpClass = new ClassAnalyzer(classSyntax, namespaceName, semanticModel);
                        _classes.Add(cSharpClass);
                    }
                }
            }
        }

        public void VisitClasses(Action<ClassAnalyzer> visitor, Predicate<ClassAnalyzer>? filter = null)
        {
            var actualFilter = filter ?? (_ => true);
            foreach (var @class in _classes.Where(@class => actualFilter(@class)))
            {
                visitor(@class);
            }
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

            var compilation = CSharpCompilation.Create(null, syntaxTrees);
            return new SourceAnalyzer(compilation, syntaxTrees);
        }
    }
}
