using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5.Lib.Logging;
public sealed class LogScope {
    private const string DefaultScopeName = "Default";
    private const LogLevel DefaultMinLogLevel = LogLevel.Info;

    public static readonly LogScope Default = new(DefaultScopeName, DefaultMinLogLevel);

    public readonly string Name;
    public LogLevel MinLogLevel;
    public LogScope(string name, LogLevel minLogLevel = DefaultMinLogLevel) {
        this.Name = name;
        this.MinLogLevel = minLogLevel;
    }

    public override string ToString() { return this.Name; }

}
