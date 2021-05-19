using System.IO;
using System.Linq;
using CodeAnalyzer.Analysis;
using CodeAnalyzer.Analysis.Cohesion;
using NUnit.Framework;

namespace CodeAnalyzer.Tests.Analysis.Cohesion
{
    public static class ClassCohesionFixture
    {
        public static ClassCohesionAnalyzer BuildMethodAnalyzer(string className)
        {
            var @class = SourceAnalyzer.FromFile(GetTestFile(className))
                .DependencyGraph.Nodes.First(n => n.Identifier.EndsWith(className))
                .Class;

            return ClassCohesionAnalyzer.FromClass(@class);
        }

        private static string GetTestFile(string className) =>
            Path.Combine(TestContext.CurrentContext.TestDirectory, "Analysis", "Cohesion", "TestClasses", $"{className}.cs");
    }
}
