namespace CodeAnalyzer.Tests.Analysis.Cohesion.TestClasses
{
    public class ClassWithTwoMethodsAccessingTheSameField
    {
        private readonly string _accessedField;

        public ClassWithTwoMethodsAccessingTheSameField(string accessedField)
        {
            _accessedField = accessedField;
        }

        public string FirstAccess() => _accessedField.ToUpper();

        internal int SecondAccess() => _accessedField.Length;
    }
}
