using System.Runtime.CompilerServices;

namespace H5.Lib.Logging;
/// <summary>
/// Logging levels.
/// More severe levels have lower values.
/// Values match syslog values, though not all syslog values are included
/// </summary>
public enum LogLevel : byte {
	/// <summary>Error conditions</summary>
	Error = 3,
	/// <summary>Warning conditions</summary>
	Warn = 4,
	/// <summary>Informational messages: Confirmation that the program is working as expected</summary>
	Info = 6,
	/// <summary>Debug-level messages: Messages that contain information normally of use only when debugging a program.</summary>
	Debug = 7,
}

/// <summary>Extentions for <see cref="LogLevel"/></summary>
public static class LogLevelExtentions {
	private static readonly string[] LogLevelNames = GetLogLevelNames();
	private static string[] GetLogLevelNames() {
		LogLevel[] logLevels = Enum.GetValues<LogLevel>();
		int maxLoglevelInt = logLevels.Select(x => (int)x).Max();
		string[] result = new string[maxLoglevelInt + 1];
		foreach (LogLevel level in logLevels) {
			result[(int)level] = level.ToString();
		}
		return result;
	}
	/// <summary>Faster version of <see cref="LogLevel"/>.ToString()</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string Name(this LogLevel level) {
		return LogLevelNames[(int)level];
	}
}

