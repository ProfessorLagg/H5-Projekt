namespace H5.Lib.Logging;
/// <summary>Named scope used to tag log messages</summary>
public sealed class LogScope {
	private const string DefaultScopeName = "Default";
	private const LogLevel DefaultMinLogLevel = LogLevel.Info;

	/// <summary>Default LogScope. Uses <see cref="DefaultScopeName"/> and <see cref="DefaultMinLogLevel"/></summary>
	public static readonly LogScope Default = new(DefaultScopeName, DefaultMinLogLevel);

	/// <summary>
	/// Name of the <see cref="LogScope"/>.
	/// This is what log messages should be tagged with when written
	/// </summary>
	public readonly string Name;
	/// <summary>Lowest (least severe) <see cref="LogLevel"/> to write.</summary>
	public LogLevel MinLogLevel;
	/// <summary>
	/// Creates a new instance of a <see cref="LogScope"/>
	/// </summary>
	/// <param name="name">Name of the <see cref="LogScope"/>.
	/// This is what log messages should be tagged with when written
	/// </param>
	/// <param name="minLogLevel">
	/// Lowest (least severe) <see cref="LogLevel"/> to write.
	/// Defaults to <see cref="LogScope.DefaultMinLogLevel"/>
	/// </param>
	public LogScope(string name, LogLevel minLogLevel = DefaultMinLogLevel) {
		this.Name = name;
		this.MinLogLevel = minLogLevel;
	}

}
