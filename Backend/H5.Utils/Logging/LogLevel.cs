using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace H5.Lib.Logging;
public enum LogLevel : byte {
    Error = 3,
    Warn = 4,
    Info = 6,
    Debug = 7,
}


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
    /// <summary>Faster version of <see cref="LogLevel.ToString()"/></summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Name(this LogLevel level) {
        return LogLevelNames[(int)level];
    }
}

