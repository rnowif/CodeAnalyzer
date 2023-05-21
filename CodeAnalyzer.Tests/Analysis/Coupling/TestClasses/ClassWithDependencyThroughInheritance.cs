namespace CodeAnalyzer.Tests.Analysis.Coupling.TestClasses;

public class ClassWithDependencyThroughInheritance : SuperClass, ISuperInterface
{

}

public class SuperClass {}
public interface ISuperInterface {}