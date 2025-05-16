using H5.Lib.Utils;

using System.Text;

namespace H5.Lib.Logging;
#nullable enable
public sealed class FileLog : ILogDestination {
	public static readonly string DefaultDirectoryPath = Path.Join(PathUtils.ExeDirectory.FullName, "Logs");
	public static readonly DirectoryInfo DefaultDirectory = new DirectoryInfo(DefaultDirectoryPath);
	public static readonly Encoding DefaultEncoding = Encoding.UTF8;

	private static readonly int BufferSize = Environment.SystemPageSize;

	#region Instance Data
	private DirectoryInfo LogDirectory;
	private Encoding Encoding;
	private DateTime LogFileDate = DateTime.MinValue;
	private FileStream? LogFileStream = null;
	private object WriteLock = new();
	#endregion

	#region Constructors
	public FileLog(DirectoryInfo logDir, Encoding encoding) {
		LogDirectory = logDir;
		Encoding = encoding;
	}
	public FileLog(DirectoryInfo logDir) : this(logDir, DefaultEncoding) { }
	public FileLog() : this(DefaultDirectory, DefaultEncoding) { }
	public FileLog(Encoding encoding) : this(DefaultDirectory, encoding) { }
	public FileLog(string logDirPath) : this(new DirectoryInfo(logDirPath), DefaultEncoding) { }
	public FileLog(string logDirPath, Encoding encoding) : this(new DirectoryInfo(logDirPath), encoding) { }
	#endregion

	#region Private Methods
	private static string CalculateLogfilePath(DirectoryInfo logDir, DateTime timestamp) {
		return Path.Join(logDir.FullName, $"{timestamp.ToString("yyyy-MM-dd")}.log");
	}
	private void CloseLogFileStream() {
		if (this.LogFileStream is null) { return; }
		this.LogFileStream.Flush();
		try { this.LogFileStream.Close(); } catch { /*I don't really care about Exceptions that happen when trying to close a file*/}
	}
	private void NewLogFile() {
		this.CloseLogFileStream();
		this.LogFileDate = DateTime.Today;
		string logFilePath = CalculateLogfilePath(this.LogDirectory, this.LogFileDate);
		this.LogDirectory.EnsureExists();
		this.LogFileStream = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read, BufferSize, false);
	}
	private FileStream EnsureLogFile() {
		if (this.LogFileDate != DateTime.Today) { NewLogFile(); }
		return this.LogFileStream!;
	}
	#endregion

	#region Interface Implementation
	public bool Equals(ILogDestination? other) {
		if (other is null || other is not FileLog) { return false; }
		FileLog otherFileLogger = (FileLog)other;
		return this.LogDirectory.Equals(otherFileLogger.LogDirectory);
	}

	public void Write(LogMessage logMessage) {
		lock (WriteLock) {
			FileStream stream = this.EnsureLogFile();
			StreamWriter streamWriter = new(stream, this.Encoding, -1, true);
			streamWriter.Write(logMessage.Timestamp.ToString("yyyy-MM-dd HH:mm:sszzz"));
			streamWriter.Write("  ");

			streamWriter.Write(logMessage.Scope.Name);
			streamWriter.Write(':');

			streamWriter.Write(logMessage.Level.ToString());
			streamWriter.Write("  ");
			streamWriter.WriteLine(logMessage.Message.Trim().EscapeWhitespace());
			streamWriter.Close();
		}
	}
	#endregion
}
#nullable disable