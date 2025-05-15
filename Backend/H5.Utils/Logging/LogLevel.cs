using System;
using System.Collections.Generic;
using System.Linq;
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
        List<string> result = new();
        SortedSet<int> isertedIndexes = new();
        foreach (LogLevel level in Enum.GetValues<LogLevel>()) {
            int idx = (int)level;

            result.Insert(idx, level.ToString());
            isertedIndexes.Add(idx);
        }

        for (int i = 0; i < result.Count; i++) {
            if (!isertedIndexes.Contains(i)) {
                result[i] = string.Empty;
            }
        }
        return result.ToArray();
    }
    public static string Name(this LogLevel level) {
        return LogLevelNames[(int)level];
    }
}

