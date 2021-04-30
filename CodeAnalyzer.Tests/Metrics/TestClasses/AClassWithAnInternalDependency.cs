namespace CodeAnalyzer.Tests.Metrics.TestClasses
{
    public class AClassWithAnInternalDependency
    {
        private readonly TheDependency _theDependency;

        public AClassWithAnInternalDependency(TheDependency theDependency)
        {
            _theDependency = theDependency;
        }
    }

    public class TheDependency
    {

    }
}
