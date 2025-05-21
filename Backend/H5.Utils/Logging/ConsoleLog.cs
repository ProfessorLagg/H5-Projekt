namespace H5.Lib.Logging;
/// <summary>
/// ILogDestination for logging to the console window.
/// Warning! Very slow on windows, which cannot be fixed since the slowness is in conhost
/// </summary>
public sealed class ConsoleLog : ILogDestination {
	private static readonly int BufferSize = Environment.SystemPageSize;
	private object WriteLock = new();
	private Stream StdOut = Console.OpenStandardOutput();

	/// <inheritdoc/>
	public bool Equals(ILogDestination other) {
		if (other is null || other is not ConsoleLog) { return false; }
		return true;
	}

	/// <inheritdoc/>
	public void Write(LogMessage logMessage) {
		lock (WriteLock) {
			string msg = $"{Environment.NewLine}{logMessage.Scope.Name}:{logMessage.Level.Name()}{Environment.NewLine}{logMessage.Message}{Environment.NewLine}";
			byte[] msgBytes = Console.OutputEncoding.GetBytes(msg);
			StdOut.Write(msgBytes);
			StdOut.Flush();
		}
	}

	/// <summary>Destructor</summary>
	~ConsoleLog() {
		this.StdOut.Close();
	}
}