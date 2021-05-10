namespace CodeAnalyzer.Tests.Methods.TestClasses
{
    public class ClassWithTwoMethodsAccessingTheSameField
    {
        private readonly string _accessedField;

        public ClassWithTwoMethodsAccessingTheSameField(string accessedField)
        {
            _accessedField = accessedField;
        }

        public string FirstAccess() => _accessedField.ToUpper();

        int SecondAccess() => _accessedField.Length;
    }
}
