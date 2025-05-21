using H5.Lib.Utils;

using System.Text;

namespace H5.Lib.Logging;
#nullable enable
/// <summary>Writes <see cref="LogMessage"/>'s to timestamp named files in a directory</summary>
public sealed class FileLog : ILogDestination {
	/// <summary>Path to default directory. Used when no directory was passed to constructor</summary>
	public static readonly string DefaultDirectoryPath = Path.Join(PathUtils.ExeDirectory.FullName, "Logs");
	/// <summary><see cref="DirectoryInfo"/> of <see cref="FileLog.DefaultDirectoryPath"/>. Used when no directory was passed to constructor</summary>
	public static readonly DirectoryInfo DefaultDirectory = new DirectoryInfo(DefaultDirectoryPath);
	/// <summary>Default <see cref="Encoding"/> for log files. Used when no <see cref="Encoding"/> was passed constructor</summary>
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
	/// <summary>Creates a new instance of the <see cref="FileLog"/> class</summary>
	/// <param name="logDir"><see cref="DirectoryInfo"/> of directory to place logfiles in</param>
	/// <param name="encoding"><see cref="Encoding"/> text encoding to write logfiles in</param>
	public FileLog(DirectoryInfo logDir, Encoding encoding) {
		LogDirectory = logDir;
		Encoding = encoding;
	}

	/// <summary><inheritdoc cref="FileLog(DirectoryInfo, Encoding)"/> using <see cref="FileLog.DefaultEncoding"/></summary>
	/// <param name="logDir"><see cref="DirectoryInfo"/> of directory to place logfiles in</param>
	public FileLog(DirectoryInfo logDir) : this(logDir, DefaultEncoding) { }

	/// <summary><inheritdoc cref="FileLog(DirectoryInfo, Encoding)"/> using <see cref="FileLog.DefaultDirectory"/> and <see cref="FileLog.DefaultEncoding"/></summary>
	public FileLog() : this(DefaultDirectory, DefaultEncoding) { }

	/// <summary><inheritdoc cref="FileLog(DirectoryInfo, Encoding)"/> using <see cref="FileLog.DefaultDirectory"/></summary>
	/// <param name="encoding"><see cref="Encoding"/> text encoding to write logfiles in</param>
	public FileLog(Encoding encoding) : this(DefaultDirectory, encoding) { }

	/// <summary><inheritdoc cref="FileLog(string, Encoding)"/> using <see cref="FileLog.DefaultEncoding"/></summary>
	/// <param name="logDirPath">Path of directory to place logfiles in</param>
	public FileLog(string logDirPath) : this(new DirectoryInfo(logDirPath), DefaultEncoding) { }

	/// <inheritdoc cref="FileLog(DirectoryInfo, Encoding)"/>
	/// <param name="logDirPath">Path of directory to place logfiles in</param>
	/// <param name="encoding"><inheritdoc/></param>
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
	/// <inheritdoc/>
	public bool Equals(ILogDestination? other) {
		if (other is null || other is not FileLog) { return false; }
		FileLog otherFileLogger = (FileLog)other;
		return this.LogDirectory.Equals(otherFileLogger.LogDirectory);
	}

	/// <inheritdoc/>
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
	/// <summary>Destructor</summary>
	~FileLog() {
		if (this.LogFileStream is not null) {
			this.LogFileStream.Close();
		}
	}
}
#nullable disable