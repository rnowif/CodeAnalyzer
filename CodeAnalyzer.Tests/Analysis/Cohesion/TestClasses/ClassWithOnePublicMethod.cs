namespace CodeAnalyzer.Tests.Analysis.Cohesion.TestClasses;

public class ClassWithOnePublicMethod
{
    private readonly string _field;

    public ClassWithOnePublicMethod(string field)
    {
        _field = field;
    }

    public string ToUpper() => _field.ToUpper();
}