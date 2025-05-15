using H5.Lib.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5.Lib.Logging;
public sealed class FileLog : ILogDestination {
    public static readonly string DefaultDirectoryPath = Path.Join(PathUtils.ExeDirectory.FullName, "Logs");
    public static readonly DirectoryInfo DefaultDirectory = new DirectoryInfo(DefaultDirectoryPath);
    private DirectoryInfo LogDirectory;

    public FileLog() : this(DefaultDirectory) { }

    public FileLog(DirectoryInfo logDir) {
        LogDirectory = logDir;
        if (!LogDirectory.Exists) { LogDirectory.Create(); }
        throw new NotImplementedException();
    }
    public FileLog(string logDirPath) : this(new DirectoryInfo(logDirPath)) { }

    public bool Equals(ILogDestination other) {
        if (other is not FileLog) { return false; }
        FileLog otherFileLogger = (FileLog)other;
        return this.LogDirectory.Equals(otherFileLogger.LogDirectory);
    }

    public void Write(LogLevel logLevel, string scope, string message, params object[] args) {
        throw new NotImplementedException();
    }
}
