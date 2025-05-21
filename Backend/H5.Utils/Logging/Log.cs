using System.Collections.Concurrent;

namespace H5.Lib.Logging;
#nullable enable
/// <summary>
/// Global class for writing <see cref="LogMessage"/>'s
/// </summary>
public static class Log {
	private static readonly ConcurrentBag<ILogDestination> Destinations = new();

	/// <summary>
	/// Writes an <see cref="LogLevel.Error"/> log message from the <see cref="LogScope.Default"/> scope
	/// </summary>
	/// <param name="message"><inheritdoc cref="Write(LogScope, LogLevel, string?)"/></param>
	public static void Error(string? message) { Write(LogLevel.Error, message); }
	/// <summary>
	/// Writes an <see cref="LogLevel.Error"/> log message
	/// </summary>
	/// <param name="message"><inheritdoc cref="Write(LogScope, LogLevel, string?)"/></param>
	/// /// <param name="scope"><inheritdoc cref="Write(LogScope, LogLevel, string?)"/></param>
	public static void Error(this LogScope scope, string? message) { scope.Write(LogLevel.Error, message); }

	/// <summary>
	/// Writes a <see cref="LogLevel.Warn"/> log message from the <see cref="LogScope.Default"/> scope
	/// </summary>
	/// <param name="message"><inheritdoc cref="Write(LogScope, LogLevel, string?)"/></param>
	public static void Warn(string? message) { Write(LogLevel.Warn, message); }
	/// <summary>
	/// Writes a <see cref="LogLevel.Warn"/> log message
	/// </summary>
	/// <param name="message"><inheritdoc cref="Write(LogScope, LogLevel, string?)"/></param>
	/// /// <param name="scope"><inheritdoc cref="Write(LogScope, LogLevel, string?)"/></param>
	public static void Warn(this LogScope scope, string? message) { scope.Write(LogLevel.Warn, message); }

	/// <summary>
	/// Writes an <see cref="LogLevel.Info"/> log message from the <see cref="LogScope.Default"/> scope
	/// </summary>
	/// <param name="message"><inheritdoc cref="Write(LogScope, LogLevel, string?)"/></param>
	public static void Info(string? message) { Write(LogLevel.Info, message); }
	/// <summary>
	/// Writes an <see cref="LogLevel.Info"/> log message
	/// </summary>
	/// <param name="message"><inheritdoc cref="Write(LogScope, LogLevel, string?)"/></param>
	/// /// <param name="scope"><inheritdoc cref="Write(LogScope, LogLevel, string?)"/></param>
	public static void Info(this LogScope scope, string? message) { scope.Write(LogLevel.Info, message); }

	/// <summary>
	/// Writes a <see cref="LogLevel.Debug"/> log message from the <see cref="LogScope.Default"/> scope
	/// </summary>
	/// <param name="message"><inheritdoc cref="Write(LogScope, LogLevel, string?)"/></param>
	public static void Debug(string? message) { Write(LogLevel.Debug, message); }
	/// <summary>
	/// Writes a <see cref="LogLevel.Debug"/> log message
	/// </summary>
	/// <param name="message"><inheritdoc cref="Write(LogScope, LogLevel, string?)"/></param>
	/// <param name="scope"><inheritdoc cref="Write(LogScope, LogLevel, string?)"/></param>
	public static void Debug(this LogScope scope, string? message) { scope.Write(LogLevel.Debug, message); }


	/// <summary>Writes a log message from the <see cref="LogScope.Default"/> scope</summary>
	/// <param name="level"><inheritdoc cref="Write(LogScope, LogLevel, string?)"/></param>
	/// <param name="message"><inheritdoc cref="Write(LogScope, LogLevel, string?)"/></param>
	public static void Write(LogLevel level, string? message) { LogScope.Default.Write(level, message); }

	/// <summary>Writes a log message</summary>
	/// <param name="level"><see cref="LogLevel"/> of the message</param>
	/// <param name="message">Message text to write</param>
	/// <param name="scope"><see cref="LogScope"/> to write message from</param>
	public static void Write(this LogScope scope, LogLevel level, string? message) {
		LogMessage lm = new(scope, level, message);
		foreach (ILogDestination dst in Destinations) {
			ThreadPool.QueueUserWorkItem<LogMessage>(dst.Write, lm, false);
		}
	}

	/// <summary>Add's a new <see cref="ILogDestination"/> to recieve <see cref="LogMessage"/>'s</summary>
	/// <param name="destination">The <see cref="ILogDestination"/> to add</param>
	/// <exception cref="ArgumentException"></exception>
	public static void AddLogDestination(ILogDestination destination) {
		foreach (ILogDestination dst in Destinations) {
			if (destination.Equals(dst)) { throw new ArgumentException($"ILogDestination {destination} already added"); }
		}
		Destinations.Add(destination);
	}
	/// <summary>Adds a new <see cref="FileLog"/> using default settings</summary>
	public static void AddFileLog() { AddLogDestination(new FileLog()); }
	/// <summary>Adds a new <see cref="FileLog"/></summary>
	/// <param name="logDirPath">Path of the directory file log writes to</param>
	public static void AddFileLog(string logDirPath) { AddFileLog(new DirectoryInfo(logDirPath)); }
	/// <summary>Adds a new <see cref="FileLog"/></summary>
	/// <param name="logDir"><see cref="DirectoryInfo"/> of the directory file log writes to</param>
	public static void AddFileLog(DirectoryInfo logDir) { AddLogDestination(new FileLog(logDir)); }

	/// <summary>Adds a new <see cref="ConsoleLog"/></summary>
	public static void AddConsoleLog() { AddLogDestination(new ConsoleLog()); }
}
#nullable disable


