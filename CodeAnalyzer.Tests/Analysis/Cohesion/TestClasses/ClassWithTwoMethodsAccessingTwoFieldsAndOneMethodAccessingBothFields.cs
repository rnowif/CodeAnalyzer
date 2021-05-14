namespace CodeAnalyzer.Tests.Analysis.Cohesion.TestClasses
{
    public class ClassWithTwoMethodsAccessingTwoFieldsAndOneMethodAccessingBothFields
    {
        private readonly string _firstAccessedField;
        private readonly string _secondAccessedField;

        public ClassWithTwoMethodsAccessingTwoFieldsAndOneMethodAccessingBothFields(string firstAccessedField, string secondAccessedField)
        {
            _firstAccessedField = firstAccessedField;
            _secondAccessedField = secondAccessedField;
        }

        public string FirstAccess() => _firstAccessedField.ToUpper();

        public string SecondAccess() => _secondAccessedField.ToUpper();

        public int BothAccesses() => _secondAccessedField.Length + _firstAccessedField.Length;
    }
}
