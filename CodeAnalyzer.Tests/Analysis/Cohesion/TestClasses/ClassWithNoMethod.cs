namespace CodeAnalyzer.Tests.Analysis.Cohesion.TestClasses;

public class ClassWithNoMethod
{
    private readonly string _field;

    public ClassWithNoMethod(string field)
    {
        _field = field;
    }
}