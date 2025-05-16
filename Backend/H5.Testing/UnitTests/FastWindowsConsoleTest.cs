namespace H5.Testing.UnitTests;
public class FastWindowsConsoleTest : IUnitTest {
	public string GetName() { return typeof(FastWindowsConsole).FullName; }

	public void Run() {
		string line = "All my homies hate conhost\r\n";
		byte[] lineBytes = Console.OutputEncoding.GetBytes(line);

		FastWindowsConsole stdout = new();
		bool sucess = stdout.TryWriteBytes(lineBytes, out uint bytesWritten);
		TestHelpers.ExpectEqual(true, sucess);
		TestHelpers.ExpectEqual(Convert.ToUInt32(lineBytes.Length), bytesWritten);
	}
}
