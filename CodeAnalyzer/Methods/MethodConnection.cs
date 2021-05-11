using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CodeAnalyzer.Methods
{
    public class MethodConnectionComparer : IEqualityComparer<MethodConnection>
    {
        public bool Equals(MethodConnection? x, MethodConnection? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x == y;
        }

        public int GetHashCode(MethodConnection obj)
        {
            return (int) obj.Type;
        }
    }
    public class MethodConnection
    {
        internal ISymbol Method1 { get; }
        internal ISymbol Method2 { get; }
        public ConnectionType Type { get; }
        public bool IsSelfConnected => Method1.Equals(Method2);

        public MethodConnection(ConnectionType type, ISymbol method1, ISymbol method2)
        {
            Method1 = method1;
            Method2 = method2;
            Type = type;
        }

        public enum ConnectionType
        {
            Direct,
            Indirect
        }

        public override bool Equals(object? obj) => Equals(this, (MethodConnection?) obj);

        public override int GetHashCode()
        {
            return HashCode.Combine(Method1, Method2, (int) Type);
        }

        // A,B == A,B == B,A
        private static bool Equals(MethodConnection? x, MethodConnection? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Type == y.Type
                   && (x.Method1.Equals(y.Method1) && x.Method2.Equals(y.Method2) || x.Method2.Equals(y.Method1) && x.Method1.Equals(y.Method2));
        }

        public static bool operator ==(MethodConnection? left, MethodConnection? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MethodConnection? left, MethodConnection? right)
        {
            return !Equals(left, right);
        }
    }
}
