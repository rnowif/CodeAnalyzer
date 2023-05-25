using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CodeAnalyzer.Analysis.Coupling;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace CodeAnalyzer.Analysis;

public class SourceAnalyzer
{
    public DependencyGraph DependencyGraph { get; }

    private SourceAnalyzer(IEnumerable<DocumentModels> documents)
    {
        var classes = new List<ClassAnalyzer>();

        foreach (var document in documents)
        {
            var root = document.SyntaxTree.GetCompilationUnitRoot();

            foreach (var namespaceSyntax in root.Members.OfType<BaseNamespaceDeclarationSyntax>())
            {
                var namespaceName = namespaceSyntax.Name.ToString();
                classes.AddRange(namespaceSyntax.Members.OfType<ClassDeclarationSyntax>()
                    .Select(classSyntax => new ClassAnalyzer(classSyntax, namespaceName, document.SemanticModel)));
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

        var compilation = CSharpCompilation.Create(null, syntaxTrees);
        var models = compilation.SyntaxTrees
            .Select(syntaxTree => new DocumentModels
            {
                SemanticModel = compilation.GetSemanticModel(syntaxTree),
                SyntaxTree = syntaxTree
            });

        return new SourceAnalyzer(models);
    }
    
    public static async Task<SourceAnalyzer> FromSolution(string solutionFile)
    {
        MSBuildLocator.RegisterDefaults();
        using var workspace = MSBuildWorkspace.Create();
        var solution = await workspace.OpenSolutionAsync(solutionFile);
        
        var documents = solution.Projects.SelectMany(p => p.Documents);

        var models = new List<DocumentModels>();
        
        foreach (var document in documents)
        {
            var syntaxTree = await document.GetSyntaxTreeAsync();
            var semanticModel = await document.GetSemanticModelAsync();

            if (syntaxTree != null && semanticModel != null)
            {
                models.Add(new DocumentModels
                {
                    SyntaxTree = syntaxTree,
                    SemanticModel = semanticModel
                });
            }
        }
        
        return new SourceAnalyzer(models);
    }

    private class DocumentModels
    {
        public SyntaxTree SyntaxTree { get; init; } = null!;
        public SemanticModel SemanticModel { get; init; } = null!;
    }
}
