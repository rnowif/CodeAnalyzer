using System.IO;
using System.Linq;
using CodeAnalyzer.Metrics;
using CodeAnalyzer.Tests.Metrics.TestClasses;
using CodeAnalyzer.Tree;
using FluentAssertions;
using NUnit.Framework;

namespace CodeAnalyzer.Tests.Metrics
{
    [TestFixture]
    public class DependencyAnalyzerTests
    {
        [TestCase(nameof(EmptyClass), 0)]
        [TestCase(nameof(ClassWithOnlySystemDependencies), 0)]
        [TestCase(nameof(AClassWithAnInternalDependency), 1)]
        [TestCase(nameof(AClassWithAnInternalGenericDependency), 1)]
        [TestCase(nameof(AClassWithSeveralInternalDependencies), 7)]
        [TestCase(nameof(ClassWithDependencyThroughInheritance), 2)]
        public void TestNumberOfInternalDependencies(string className, int expectedDependencies)
        {
            var tree = BuildTree(className);

            tree.FindDependencies(tree.Classes.First()).Count().Should().Be(expectedDependencies);
        }

        private static SourceTree BuildTree(string className)
        {
            return SourceTree.FromFile(GetTestFile(className));
        }

        private static string GetTestFile(string className) =>
            Path.Combine(TestContext.CurrentContext.TestDirectory, "Metrics", "TestClasses", $"{className}.cs");
    }
}
