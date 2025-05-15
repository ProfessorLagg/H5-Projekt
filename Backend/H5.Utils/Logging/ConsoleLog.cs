using H5.Lib.Utils;

using Microsoft.Win32.SafeHandles;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace H5.Lib.Logging;
public sealed class ConsoleLog : ILogDestination {
    private object WriteLock = new();

    public bool Equals(ILogDestination? other) {
        if (other is null || other is not ConsoleLog) { return false; }
        return true;
    }

    public void Write(LogMessage logMessage) {
        new SafeFileHandle();
        lock (WriteLock) {
            Stream stream = Console.OpenStandardOutput();
            StreamWriter streamWriter = new(stream, Console.OutputEncoding, Environment.SystemPageSize, true);
            streamWriter.Write(logMessage.Scope.Name);
            streamWriter.Write(':');
            streamWriter.Write(logMessage.Level.ToString());
            streamWriter.Write("  ");
            streamWriter.WriteLine(logMessage.Message.Trim());
            streamWriter.Close();
        }
    }
}