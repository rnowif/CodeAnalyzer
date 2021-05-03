namespace CodeAnalyzer.Tests.Metrics.TestClasses
{
    public class ClassWithDependencyThroughInheritance : SuperClass, ISuperInterface
    {

    }

    public class SuperClass {}
    public interface ISuperInterface {}
}
