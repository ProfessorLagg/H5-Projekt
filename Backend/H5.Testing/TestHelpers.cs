using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5.Testing;
internal static class TestHelpers {
    internal class ExpectEqualException : Exception {
        public ExpectEqualException(string message) : base(message) { }
        public ExpectEqualException() : base() { }
        public static ExpectEqualException Throw<T>(T expect, T found) where T : IEquatable<T> {
            throw new ExpectEqualException($"Excpected {expect}, but found {found}");
        }
    }
    public static void ExpectEqual<T>(T expect, T found) where T : IEquatable<T> {
        if (!expect.Equals(found)) { ExpectEqualException.Throw<T>(expect, found); }
    }

    internal class ExpectNotEqualException : Exception {
        public ExpectNotEqualException(string message) : base(message) { }
        public ExpectNotEqualException() : base() { }
        public static void Throw<T>(T expect, T found) where T : IEquatable<T> {
            throw new ExpectNotEqualException($"Expected not equal, but found {expect} == {found}");
        }
    }
    public static void ExpectNotEqual<T>(T expect, T found) where T : IEquatable<T> {
        if (expect.Equals(found)) { ExpectNotEqualException.Throw<T>(expect, found); }
    }
}
