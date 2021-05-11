namespace CodeAnalyzer.Tests.Methods.TestClasses
{
    public class ClassWithTwoMethodsAccessingTheSameProperty
    {
        public string AccessedProperty { get; }

        public ClassWithTwoMethodsAccessingTheSameProperty(string accessedField)
        {
            AccessedProperty = accessedField;
        }

        public string FirstAccess() => AccessedProperty.ToUpper();

        internal int SecondAccess() => AccessedProperty.Length;
    }
}
