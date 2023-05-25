namespace CodeAnalyzer.Tests.Analysis.Cohesion.TestClasses;

public class ClassWithOverloadMethods
{
    private readonly string _a;

    public ClassWithOverloadMethods(string a)
    {
        _a = a;
    }

    public int FirstAccess() => _a.Length;

    public int OverloadMethod() => OverloadMethod(0);

    private int OverloadMethod(int i) => i > 5 ? i : OverloadMethod(_a.Length + i);
}
