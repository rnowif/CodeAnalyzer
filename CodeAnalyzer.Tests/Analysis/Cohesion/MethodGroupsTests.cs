using CodeAnalyzer.Analysis.Cohesion;
using CodeAnalyzer.Tests.Analysis.Cohesion.TestClasses;
using FluentAssertions;
using NUnit.Framework;

namespace CodeAnalyzer.Tests.Analysis.Cohesion
{
    [TestFixture]
    public class MethodGroupsTests
    {
        [Test]
        public void A_Class_With_No_Method_Has_0_Group()
        {
            var analyzer = BuildMethodAnalyzer(nameof(ClassWithNoMethod));

            analyzer.ComputeLackOfCohesionOfMethod().Should().Be(0);
        }

        [Test]
        public void A_Class_With_One_Method_Has_1_Group()
        {
            var analyzer = BuildMethodAnalyzer(nameof(ClassWithOnePublicMethod));

            analyzer.ComputeLackOfCohesionOfMethod().Should().Be(1);
        }

        [Test]
        public void A_Class_With_Two_Directly_Connected_Methods_Has_1_Group()
        {
            var analyzer = BuildMethodAnalyzer(nameof(ClassWithTwoMethodsAccessingTheSameField));

            analyzer.ComputeLackOfCohesionOfMethod().Should().Be(1);
        }

        [Test]
        public void A_Class_With_Two_Unconnected_Methods_Has_2_Groups()
        {
            var analyzer = BuildMethodAnalyzer(nameof(ClassWithTwoMethodsAccessingTwoFields));

            analyzer.ComputeLackOfCohesionOfMethod().Should().Be(2);
        }

        [Test]
        public void Dangling_Methods_Should_Count_As_A_Group()
        {
            var analyzer = BuildMethodAnalyzer(nameof(ClassWithTwoMethodsIncludingADanglingOne));

            analyzer.ComputeLackOfCohesionOfMethod().Should().Be(2);
        }

        [Test]
        public void A_Class_With_Two_Unconnected_Methods_Both_Called_By_The_Same_Method_Has_1_Group()
        {
            var analyzer = BuildMethodAnalyzer(nameof(ClassWithTwoMethodsAccessingTwoFieldsAndOneMethodAccessingBothFields));

            analyzer.ComputeLackOfCohesionOfMethod().Should().Be(1);
        }

        private static ClassCohesionAnalyzer BuildMethodAnalyzer(string className) => ClassCohesionFixture.BuildMethodAnalyzer(className);
    }
}
