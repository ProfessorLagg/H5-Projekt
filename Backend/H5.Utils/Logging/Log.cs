using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace H5.Lib.Logging;
#nullable enable
public static class Log {
    private const string DefaultScope = "Default";
    private static readonly ConcurrentBag<ILogDestination> Destinations = new();

    public static void Error(string message) { Write(LogLevel.Error, message); }
    public static void Error(string scope, string message) { Write(LogLevel.Error, scope, message); }

    public static void Warn(string message) { Write(LogLevel.Warn, message); }
    public static void Warn(string scope, string message) { Write(LogLevel.Warn, scope, message); }

    public static void Info(string message) { Write(LogLevel.Info, message); }
    public static void Info(string scope, string message) { Write(LogLevel.Info, scope, message); }

    public static void Debug(string message) { Write(LogLevel.Debug, message); }
    public static void Debug(string scope, string message) { Write(LogLevel.Debug, scope, message); }

    public static void Write(LogLevel logLevel, string? message, params object?[] args) { Write(logLevel, DefaultScope, message); }

    public static void Write(LogLevel logLevel, string scope, string? message, params object?[] args) {
        Parallel.ForEach(Destinations, dst => {
            dst.Write(logLevel, scope, message);
        });
    }

    public static void AddLogDestination(ILogDestination destination) {
        foreach (ILogDestination dst in Destinations) {
            if (destination.Equals(dst)) { throw new ArgumentException($"ILogDestination {destination} already added"); }
        }
        Destinations.Add(destination);
    }

    public static void AddFileLog() { AddLogDestination(new FileLog()); }
    public static void AddFileLog(string logDirPath) { AddFileLog(new DirectoryInfo(logDirPath)); }
    public static void AddFileLog(DirectoryInfo logDir) { AddLogDestination(new FileLog(logDir)); }
}
#nullable disable


