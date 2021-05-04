using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeAnalyzer.Tests.Metrics.TestClasses;
using CodeAnalyzer.Tree;
using FluentAssertions;
using NUnit.Framework;

namespace CodeAnalyzer.Tests.Metrics
{
    [TestFixture]
    public class DependencyAnalyzerTests
    {
        [TestCase(typeof(EmptyClass), 0)]
        [TestCase(typeof(ClassWithOnlySystemDependencies), 0)]
        [TestCase(typeof(AClassWithAnInternalDependency), 1)]
        [TestCase(typeof(AClassWithAnInternalGenericDependency), 1)]
        [TestCase(typeof(AClassWithSeveralInternalDependencies), 7)]
        [TestCase(typeof(ClassWithDependencyThroughInheritance), 2)]
        public void TestNumberOfInternalDependencies(Type classType, int expectedDependencies)
        {
            FindDependencies(classType).Count().Should().Be(expectedDependencies);
        }

        private static IEnumerable<string> FindDependencies(Type classType) =>
            SourceAnalyzer.FromFile(GetTestFile(classType.Name))
                .DependencyGraph.FindDependencies(classType.FullName!);

        private static string GetTestFile(string className) =>
            Path.Combine(TestContext.CurrentContext.TestDirectory, "Metrics", "TestClasses", $"{className}.cs");
    }
}
