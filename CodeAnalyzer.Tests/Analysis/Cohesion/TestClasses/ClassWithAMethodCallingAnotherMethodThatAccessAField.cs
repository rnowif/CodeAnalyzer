namespace CodeAnalyzer.Tests.Analysis.Cohesion.TestClasses
{
    public class ClassWithAMethodCallingAnotherMethodThatAccessAField
    {
        private readonly string _accessedField;

        public ClassWithAMethodCallingAnotherMethodThatAccessAField(string accessedField)
        {
            _accessedField = accessedField;
        }

        public string FirstAccess() => _accessedField.ToUpper();

        public int SecondAccess() => FirstAccess().Length;
    }
}
