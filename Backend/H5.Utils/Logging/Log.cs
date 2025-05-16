using System.Collections.Concurrent;

namespace H5.Lib.Logging;
#nullable enable
public static class Log {
	private static readonly ConcurrentBag<ILogDestination> Destinations = new();

	public static void Error(string? message) { Write(LogLevel.Error, message); }
	public static void Error(this LogScope scope, string? message) { scope.Write(LogLevel.Error, message); }

	public static void Warn(string? message) { Write(LogLevel.Warn, message); }
	public static void Warn(this LogScope scope, string? message) { scope.Write(LogLevel.Warn, message); }

	public static void Info(string? message) { Write(LogLevel.Info, message); }
	public static void Info(this LogScope scope, string? message) { scope.Write(LogLevel.Info, message); }

	public static void Debug(string? message) { Write(LogLevel.Debug, message); }
	public static void Debug(this LogScope scope, string? message) { scope.Write(LogLevel.Debug, message); }

	public static void Write(LogLevel level, string? message) { LogScope.Default.Write(level, message); }
	public static void Write(this LogScope scope, LogLevel level, string? message) {
		LogMessage lm = new(scope, level, message);
		foreach (ILogDestination dst in Destinations) {
			ThreadPool.QueueUserWorkItem<LogMessage>(dst.Write, lm, false);
		}
	}

	public static void AddLogDestination(ILogDestination destination) {
		foreach (ILogDestination dst in Destinations) {
			if (destination.Equals(dst)) { throw new ArgumentException($"ILogDestination {destination} already added"); }
		}
		Destinations.Add(destination);
	}
	public static void AddFileLog() { AddLogDestination(new FileLog()); }
	public static void AddFileLog(string logDirPath) { AddFileLog(new DirectoryInfo(logDirPath)); }
	public static void AddFileLog(DirectoryInfo logDir) { AddLogDestination(new FileLog(logDir)); }

	public static void AddConsoleLog() { AddLogDestination(new ConsoleLog()); }
}
#nullable disable


