namespace CodeAnalyzer.Tests.Methods.TestClasses
{
    public class ClassWithNoPublicMethod
    {
        private readonly string _field;

        public ClassWithNoPublicMethod(string field)
        {
            _field = ToUpper(field);
        }

        private string ToUpper(string value) => value.ToUpper();
    }
}
