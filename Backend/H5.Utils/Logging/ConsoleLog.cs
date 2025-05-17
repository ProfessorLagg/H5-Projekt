namespace H5.Lib.Logging;
public sealed class ConsoleLog : ILogDestination {
	private static readonly int BufferSize = Environment.SystemPageSize;
	private object WriteLock = new();
	private Stream StdOut = Console.OpenStandardOutput();


	public bool Equals(ILogDestination? other) {
		if (other is null || other is not ConsoleLog) { return false; }
		return true;
	}

	public void Write(LogMessage logMessage) {
		lock (WriteLock) {
			string msg = $"{Environment.NewLine}{logMessage.Scope.Name}:{logMessage.Level.Name()}{Environment.NewLine}{logMessage.Message}{Environment.NewLine}";
			byte[] msgBytes = Console.OutputEncoding.GetBytes(msg);
			StdOut.Write(msgBytes);
			StdOut.Flush();
		}
	}

	~ConsoleLog() {
		this.StdOut.Close();
	}
}