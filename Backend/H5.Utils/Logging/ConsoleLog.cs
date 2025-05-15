using H5.Lib.Utils;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5.Lib.Logging;
#nullable enable
public sealed class ConsoleLog : ILogDestination {
    private object WriteLock = new();

    public bool Equals(ILogDestination? other) {
        if (other is null || other is not ConsoleLog) { return false; }
        return true;
    }
    public void Write(LogScope scope, LogLevel logLevel, string? message) {
        lock (WriteLock) {
            Stream stream = Console.OpenStandardOutput();
            StreamWriter streamWriter = new(stream, Console.OutputEncoding, -1, true);
            streamWriter.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:sszzz"));
            streamWriter.Write("  ");

            streamWriter.Write(scope.Name);
            streamWriter.Write(':');

            streamWriter.Write(logLevel.ToString());
            streamWriter.Write("  ");
            message = message ?? "";
            streamWriter.WriteLine(message.Trim());
            streamWriter.Close();
        }
    }
}
#nullable disable