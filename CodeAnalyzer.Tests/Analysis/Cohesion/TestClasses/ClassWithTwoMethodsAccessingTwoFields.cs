namespace CodeAnalyzer.Tests.Analysis.Cohesion.TestClasses;

public class ClassWithTwoMethodsAccessingTwoFields
{
    private readonly string _firstAccessedField;
    private readonly string _secondAccessedField;

    public ClassWithTwoMethodsAccessingTwoFields(string firstAccessedField, string secondAccessedField)
    {
        _firstAccessedField = firstAccessedField;
        _secondAccessedField = secondAccessedField;
    }

    public string FirstAccess() => _firstAccessedField.ToUpper();

    public string SecondAccess() => _secondAccessedField.ToUpper();
}