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
        public static ExpectEqualException Throw<T>(T expect, T found) {
            throw new ExpectEqualException($"Excpected {expect}, but found {found}");
        }
    }
    public static void ExpectEqual<T>(T expect, T found) {
        if (!expect.Equals(found)) { ExpectEqualException.Throw<T>(expect, found); }
    }
    public static void ExpectEqualSpans<T>(ref ReadOnlySpan<T> spanA, ref ReadOnlySpan<T> spanB) {
        if (spanA.Length != spanB.Length) ExpectEqualException.Throw(spanA.Length, spanB.Length);
        for (int i = 0; i < spanA.Length; i++) {
            if (!spanA[i].Equals(spanB[i])) {
                ExpectEqualException.Throw(spanA[i], spanB[i]);
            }
        }
    }

    internal class ExpectNotEqualException : Exception {
        public ExpectNotEqualException(string message) : base(message) { }
        public ExpectNotEqualException() : base() { }
        public static void Throw<T>(T expect, T found) {
            throw new ExpectNotEqualException($"Expected not equal, but found {expect} == {found}");
        }
    }
    public static void ExpectNotEqual<T>(T expect, T found) {
        if (expect.Equals(found)) { ExpectNotEqualException.Throw<T>(expect, found); }
    }
}
