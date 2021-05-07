using System;

namespace CodeAnalyzer.Tests.Dependencies.TestClasses
{
    public class AClassWithSeveralInternalDependencies
    {
        private static readonly D StaticDependency = new D();

        private readonly Func<A, B> _genericDependencies;
        private C FieldDependency { get; }

        public AClassWithSeveralInternalDependencies(Func<A, B> genericDependencies)
        {
            _genericDependencies = genericDependencies;
            FieldDependency = new C();
        }

        public F AMethod(E methodDependency)
        {
            var inMethodDependency = new G();

            return new F();
        }
    }

    public class A {}
    public class B {}
    public class C {}
    public class D {}
    public class E {}
    public class F {}
    public class G {}
}
