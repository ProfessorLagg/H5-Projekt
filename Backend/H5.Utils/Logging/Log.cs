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
    private static readonly ConcurrentBag<ILogDestination> Destinations = new();

    public static void Error(string? message) { Write(LogLevel.Error, message); }
    public static void Error(this LogScope scope, string? message) { scope.Write(LogLevel.Error, message); }

    public static void Warn(string? message) { Write(LogLevel.Warn, message); }
    public static void Warn(this LogScope scope, string? message) { scope.Write(LogLevel.Warn, message); }

    public static void Info(string? message) { Write(LogLevel.Info, message); }
    public static void Info(this LogScope scope, string? message) { scope.Write(LogLevel.Info, message); }

    public static void Debug(string? message) { Write(LogLevel.Debug, message); }
    public static void Debug(this LogScope scope, string? message) { scope.Write(LogLevel.Debug, message); }

    public static void Write(LogLevel logLevel, string? message) { LogScope.Default.Write(logLevel, message); }
    public static void Write(this LogScope scope, LogLevel logLevel, string? message) {
        Parallel.ForEach(Destinations, dst => {
            dst.Write(scope, logLevel, message);
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


