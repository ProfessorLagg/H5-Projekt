namespace H5.Lib.Logging;

/// <summary>Destination that <see cref="LogMessage"/>'s can be written to</summary>
public interface ILogDestination : IEquatable<ILogDestination> {
	/// <summary>
	/// Writes a <see cref="LogMessage"/> to this <see cref="ILogDestination"></see>
	/// </summary>
	/// <param name="logMessage">The <see cref="LogMessage"/> to write</param>
	public void Write(LogMessage logMessage);
}
