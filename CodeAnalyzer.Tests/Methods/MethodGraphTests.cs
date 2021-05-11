using System.IO;
using System.Linq;
using CodeAnalyzer.Analyzer;
using CodeAnalyzer.Methods;
using CodeAnalyzer.Tests.Methods.TestClasses;
using FluentAssertions;
using NUnit.Framework;

namespace CodeAnalyzer.Tests.Methods
{
    [TestFixture]
    public class MethodGraphTests
    {

        [Test]
        public void Class_With_No_Public_Method_Has_No_Direct_Connection()
        {
            var graph = BuildMethodGraph(nameof(ClassWithNoPublicMethod));

            graph.CountVisible.Should().Be(0);
            graph.CountDirectConnections.Should().Be(0);
        }

        [Test]
        public void Class_With_One_Public_Method_Has_No_Direct_Connection()
        {
            var graph = BuildMethodGraph(nameof(ClassWithOnePublicMethod));

            graph.CountVisible.Should().Be(1);
            graph.CountDirectConnections.Should().Be(0);
        }

        [Test]
        public void Class_With_Two_Visible_Methods_Accessing_The_Same_Field_Has_One_Direct_Connection()
        {
            var graph = BuildMethodGraph(nameof(ClassWithTwoMethodsAccessingTheSameField));

            graph.CountVisible.Should().Be(2);
            graph.CountDirectConnections.Should().Be(1);
        }

        [Test]
        public void Class_With_Two_Visible_Methods_Accessing_The_Same_Property_Has_One_Direct_Connection()
        {
            var graph = BuildMethodGraph(nameof(ClassWithTwoMethodsAccessingTheSameProperty));

            graph.CountVisible.Should().Be(2);
            graph.CountDirectConnections.Should().Be(1);
        }

        [Test]
        public void Class_With_Two_Methods_Accessing_Two_Fields_And_One_Method_Accessing_Both_Fields_Has_Two_Direct_Connections()
        {
            var graph = BuildMethodGraph(nameof(ClassWithTwoMethodsAccessingTwoFieldsAndOneMethodAccessingBothFields));

            graph.CountVisible.Should().Be(3);
            graph.CountDirectConnections.Should().Be(2);
        }

        [Test]
        public void Class_With_A_Methods_Calling_Another_Method_That_Access_A_Field_Has_A_Direct_Connection()
        {
            var graph = BuildMethodGraph(nameof(ClassWithAMethodCallingAnotherMethodThatAccessAField));

            graph.CountVisible.Should().Be(2);
            graph.CountDirectConnections.Should().Be(1);
        }

        private static MethodGraph BuildMethodGraph(string className)
        {
            var @class = SourceAnalyzer.FromFile(GetTestFile(className))
                .DependencyGraph.Nodes.First(n => n.Identifier.EndsWith(className))
                .Class;

            return MethodGraph.FromClass(@class);
        }

        private static string GetTestFile(string className) =>
            Path.Combine(TestContext.CurrentContext.TestDirectory, "Methods", "TestClasses", $"{className}.cs");
    }
}
