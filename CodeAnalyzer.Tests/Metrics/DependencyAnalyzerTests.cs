using System.IO;
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

        [Test]
        public void Empty_Class_Have_No_Dependency()
        {
            var graph = BuildDependencyGraph(nameof(EmptyClass));

            graph.FindDependencies(typeof(EmptyClass).FullName!).Should().BeEmpty();
        }

        [Test]
        public void System_Dependencies_Do_Not_Count_Towards_Dependencies()
        {
            var graph = BuildDependencyGraph(nameof(ClassWithOnlySystemDependencies));

            graph.FindDependencies(typeof(EmptyClass).FullName!).Should().BeEmpty();
        }

        [Test]
        public void Dependencies_Can_Be_Generic_In_Constructor_Methods_Fields_Or_Body()
        {
            var graph = BuildDependencyGraph(nameof(AClassWithSeveralInternalDependencies));

            graph.FindDependencies(typeof(AClassWithSeveralInternalDependencies).FullName!)
                .Should()
                .BeEquivalentTo(
                    typeof(A).FullName,
                    typeof(B).FullName,
                    typeof(C).FullName,
                    typeof(D).FullName,
                    typeof(E).FullName,
                    typeof(F).FullName,
                    typeof(G).FullName
                );

            graph.FindReferences(typeof(A).FullName!)
                .Should().BeEquivalentTo(typeof(AClassWithSeveralInternalDependencies).FullName);
        }

        [Test]
        public void Inheritance_And_Implementation_Count_As_Dependencies()
        {
            var graph = BuildDependencyGraph(nameof(ClassWithDependencyThroughInheritance));

            graph.FindDependencies(typeof(ClassWithDependencyThroughInheritance).FullName!)
                .Should()
                .BeEquivalentTo(typeof(SuperClass).FullName, typeof(ISuperInterface).FullName);

            graph.FindReferences(typeof(SuperClass).FullName!)
                .Should().BeEquivalentTo(typeof(ClassWithDependencyThroughInheritance).FullName);
        }

        private static DependencyGraph BuildDependencyGraph(string className) => SourceAnalyzer.FromFile(GetTestFile(className)).DependencyGraph;

        private static string GetTestFile(string className) =>
            Path.Combine(TestContext.CurrentContext.TestDirectory, "Metrics", "TestClasses", $"{className}.cs");
    }
}
