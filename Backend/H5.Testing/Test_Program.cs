using H5.Testing.UnitTests;

using System.Text;

namespace H5.Testing;

internal class Test_Program {
    static void Main(string[] args) {
        Console.WriteLine($"Default Encoding: {Encoding.Default}");
        UnitTestRunner testRunner = new();
        testRunner.AddTest<UnitTests.SFC32>();
        testRunner.AddTest<UnitTests.HttpStdMethod>();
        testRunner.AddTest<UnitTests.IniFile>();
        testRunner.Run();
    }
}
