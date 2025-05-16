namespace H5.Lib.Logging;
#nullable enable
public interface ILogDestination : IEquatable<ILogDestination> {
	public void Write(LogMessage logMessage);
}
#nullable disable