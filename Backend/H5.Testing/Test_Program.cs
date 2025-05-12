using H5.Testing.UnitTests;

namespace H5.Testing;

internal class Test_Program {
    static void Main(string[] args) {
        UnitTestRunner testRunner = new();
        testRunner.AddTest<UnitTests.SFC32>();
        testRunner.AddTest<UnitTests.HttpStdMethod>();
        testRunner.Run();
    }
}
