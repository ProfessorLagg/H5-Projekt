namespace H5.Lib.Logging;
/// <summary>/// Represents a singlular log message that can be written to multiple sources/// </summary>
public sealed record class LogMessage {
	/// <summary>Timestamp for when the <see cref="LogMessage"/> was created</summary>
	public readonly DateTime Timestamp;
	/// <summary>Scope the <see cref="LogMessage"/> was created from</summary>
	public readonly LogScope Scope;
	/// <summary><see cref="LogLevel" /> of the <see cref="LogMessage"/></summary>
	public readonly LogLevel Level;
	/// <summary>The message to write</summary>
	public readonly string Message;

#nullable enable
	/// <summary>
	/// Creates a new LogMessage with timestamp set to <see cref="DateTime.Now"/>
	/// </summary>
	/// <param name="scope">The <see cref="LogScope"/> to write from</param>
	/// <param name="level">Level of the <see cref="LogMessage"/></param>
	/// <param name="message">The message to write</param>
	public LogMessage(LogScope scope, LogLevel level, string? message) {
		Timestamp = DateTime.Now;
		Scope = scope;
		Level = level;
		Message = message ?? string.Empty;
	}
#nullable disable
}
