using System.Text;

namespace H5.Testing;

internal class Test_Program {
	static void Main(string[] args) {
		Console.WriteLine($"Default Encoding: {Encoding.Default}");
		UnitTestRunner testRunner = new();
		testRunner.AddTest<UnitTests.SFC32Test>();
		testRunner.AddTest<UnitTests.HttpStdMethodTest>();
		testRunner.AddTest<UnitTests.IniFileTest>();
		testRunner.AddTest<UnitTests.ApiSettingsTest>();
		testRunner.AddTest<UnitTests.CustomLoggerTest>();

		testRunner.Run();
	}
}