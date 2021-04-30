using System;
using System.Collections.Generic;

namespace CodeAnalyzer.Tests.Metrics.TestClasses
{
    public class ClassWithOnlySystemDependencies
    {
        private readonly int _someInteger;

        public Guid GuidProperty { get; }

        public ClassWithOnlySystemDependencies(int someInteger, Guid guidProperty)
        {
            _someInteger = someInteger;
            GuidProperty = guidProperty;
        }

        public IEnumerable<string> AMethodWithParameters(string stringFromSystem, List<string> list)
        {
            return new List<string> {stringFromSystem};
        }
    }
}
