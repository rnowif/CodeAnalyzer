using System.IO;
using System.Linq;
using CodeAnalyzer.Analysis;
using CodeAnalyzer.Analysis.Cohesion;
using CodeAnalyzer.Tests.Analysis.Cohesion.TestClasses;
using FluentAssertions;
using NUnit.Framework;

namespace CodeAnalyzer.Tests.Analysis.Cohesion
{
    [TestFixture]
    public class MethodAnalyzerTests
    {

        [Test]
        public void Class_With_No_Public_Method_Has_No_Direct_Or_Indirect_Connection()
        {
            var analyzer = BuildMethodAnalyzer(nameof(ClassWithNoPublicMethod));

            analyzer.CountVisible.Should().Be(0);
            analyzer.CountDirectConnections.Should().Be(0);
            analyzer.CountIndirectConnections.Should().Be(0);
        }

        [Test]
        public void Class_With_One_Public_Method_Has_No_Direct_Or_Indirect_Connection()
        {
            var analyzer = BuildMethodAnalyzer(nameof(ClassWithOnePublicMethod));

            analyzer.CountVisible.Should().Be(1);
            analyzer.CountDirectConnections.Should().Be(0);
            analyzer.CountIndirectConnections.Should().Be(0);
        }

        [Test]
        public void Class_With_Two_Visible_Methods_Accessing_The_Same_Field_Has_One_Direct_Connection()
        {
            var analyzer = BuildMethodAnalyzer(nameof(ClassWithTwoMethodsAccessingTheSameField));

            analyzer.CountVisible.Should().Be(2);
            analyzer.CountDirectConnections.Should().Be(1);
            analyzer.CountIndirectConnections.Should().Be(0);
        }

        [Test]
        public void Class_With_Two_Visible_Methods_Accessing_The_Same_Property_Has_One_Direct_Connection()
        {
            var analyzer = BuildMethodAnalyzer(nameof(ClassWithTwoMethodsAccessingTheSameProperty));

            analyzer.CountVisible.Should().Be(2);
            analyzer.CountDirectConnections.Should().Be(1);
            analyzer.CountIndirectConnections.Should().Be(0);
        }

        [Test]
        public void Class_With_Two_Methods_Accessing_Two_Fields_And_One_Method_Accessing_Both_Fields_Has_Two_Direct_Connections_And_One_Indirect_Connection()
        {
            var analyzer = BuildMethodAnalyzer(nameof(ClassWithTwoMethodsAccessingTwoFieldsAndOneMethodAccessingBothFields));

            analyzer.CountVisible.Should().Be(3);
            analyzer.CountDirectConnections.Should().Be(2);
            analyzer.CountIndirectConnections.Should().Be(1);
        }

        [Test]
        public void Class_With_A_Methods_Calling_Another_Method_That_Access_A_Field_Has_A_Direct_Connection()
        {
            var analyzer = BuildMethodAnalyzer(nameof(ClassWithAMethodCallingAnotherMethodThatAccessAField));

            analyzer.CountVisible.Should().Be(2);
            analyzer.CountDirectConnections.Should().Be(1);
            analyzer.CountIndirectConnections.Should().Be(0);
        }

        private static ClassCohesionAnalyzer BuildMethodAnalyzer(string className)
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
