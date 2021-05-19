namespace CodeAnalyzer.Tests.Analysis.Cohesion.TestClasses
{
    public class ClassWithTwoMethodsIncludingADanglingOne
    {
        private readonly string _firstAccessedField;

        public ClassWithTwoMethodsIncludingADanglingOne(string firstAccessedField)
        {
            _firstAccessedField = firstAccessedField;
        }

        public string FirstAccess() => _firstAccessedField.ToUpper();

        public string DanglingMethod() => "Some value";
    }
}
