using H5.Lib.Logging;
namespace H5.Testing.UnitTests;
public sealed class CustomLoggerTest : IUnitTest {
	public string GetName() { return typeof(H5.Lib.Logging.Log).FullName; }

	private static void TestLogLevelNames() {
		foreach (LogLevel level in Enum.GetValues<LogLevel>()) {
			TestHelpers.ExpectEqual(level.ToString(), level.Name());
		}
	}

	public void Run() {
		Log.AddConsoleLog();
		Log.AddFileLog();

		Log.Debug($"{Environment.StackTrace}");
		Log.Info($"{Environment.StackTrace}");
		Log.Warn($"{Environment.StackTrace}");
		Log.Error($"{Environment.StackTrace}");


		LogScope scope = new("TestScope", LogLevel.Debug);
		scope.Debug($"{Environment.StackTrace}");
		scope.Info($"{Environment.StackTrace}");
		scope.Warn($"{Environment.StackTrace}");
		scope.Error($"{Environment.StackTrace}");

		TestLogLevelNames();
	}
}
