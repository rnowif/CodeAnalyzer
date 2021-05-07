namespace CodeAnalyzer.Tests.Dependencies.TestClasses
{
    public class ClassWithDependencyThroughInheritance : SuperClass, ISuperInterface
    {

    }

    public class SuperClass {}
    public interface ISuperInterface {}
}
