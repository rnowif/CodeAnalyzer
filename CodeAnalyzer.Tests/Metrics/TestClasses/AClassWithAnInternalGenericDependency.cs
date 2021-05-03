using System.Collections.Generic;

namespace CodeAnalyzer.Tests.Metrics.TestClasses
{
    public class AClassWithAnInternalGenericDependency
    {
        private readonly IEnumerable<TheGenericDependency> _theDependency;

        public AClassWithAnInternalGenericDependency(IEnumerable<TheGenericDependency> theDependency)
        {
            _theDependency = theDependency;
        }
    }

    public class TheGenericDependency
    {

    }
}
