
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

using H5.Lib.Utils;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace H5.Lib;

public sealed class FileLoggerConfiguration {
    /// <summary>The number of fractional seconds to include in the timestamp strings</summary>
    [Range(0, 7)] public int IncludeFractionalSeconds = 0;

    /// <summary>If the timestamp strings should include time zone information</summary>
    public bool IncludeTimeZone = true;
    public string TimeStampFormat {
        get {
            string format = @"yyyy\MM\DD HH:mm:ss";
            if (IncludeFractionalSeconds > 0) { format = format + '.' + new string('f', IncludeFractionalSeconds); }
            if (IncludeTimeZone) { format = format + "zzz"; }
            return format;
        }
    }

    public DirectoryInfo LogDirectory = new(Path.Join(PathUtils.ExeDirectory.FullName, "Log"));
    public Encoding Encoding = Encoding.UTF8;
    public FileInfo GetLogFile() {
        const string FileNameTimestampFormat = "yyyy\\-MM\\-dd";
        string logFileName = DateTime.Now.ToString(FileNameTimestampFormat, CultureInfo.InvariantCulture) + ".log";
        string logFilePath = Path.Join(this.LogDirectory.FullName, logFileName);
        return new FileInfo(logFilePath);
    }
}

#nullable enable
public sealed class FileLogger(string name, Func<FileLoggerConfiguration> GetCurrentConfig) : ILogger, IDisposable {

    private FileInfo? LogFile = null!;
    private FileStream? LogStream = null!;
    private void CloseLogStream() {
        if (this.LogStream is not null) {
            this.LogStream.Flush();
            this.LogStream.Dispose();
            LogStream = null;
        }
    }
    private void OpenNewLog(FileInfo newLogFile) {
        CloseLogStream();
        this.LogFile = newLogFile;
        LogFile.Directory!.EnsureExists();
        this.LogStream = LogFile.Open(FileMode.Append, FileAccess.Write, FileShare.Read);
    }
    private Stream EnsureLogWriteStream() {
        FileInfo newLogFile = Config.GetLogFile();
        if (LogFile is null || LogStream is null || !LogStream.CanWrite || !LogStream.CanSeek || !LogFile.Exists) {
            OpenNewLog(newLogFile);
            return this.LogStream!;
        }

        if (!LogFile.FullName.Equals(newLogFile.FullName, StringComparison.InvariantCultureIgnoreCase)) {
            LogFile = newLogFile;
            OpenNewLog(newLogFile);
        }

        this.LogStream.Seek(0, SeekOrigin.End);
        return this.LogStream!;
    }


    private FileLoggerConfiguration Config { get { return GetCurrentConfig.Invoke(); } }
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;
    public bool IsEnabled(LogLevel logLevel) => default!;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
        if (!IsEnabled(logLevel)) { return; }
        Stream stream = this.EnsureLogWriteStream();
        StringBuilder lineBuilder = new();
        lineBuilder.Append(DateTime.Now.ToString(Config.TimeStampFormat)); lineBuilder.Append("  ");
        lineBuilder.Append(eventId.ToString()); lineBuilder.Append("  ");
        lineBuilder.Append(logLevel.ToString()); lineBuilder.Append("  ");
        lineBuilder.Append(name); lineBuilder.Append("  ");
        lineBuilder.Append(formatter.Invoke(state, exception).Trim());
        lineBuilder.EscapeWhitespace();

        ReadOnlySpan<byte> lineBytes = Config.Encoding.GetBytes(lineBuilder.ToString());
        stream.Write(lineBytes);
    }

    public void Dispose() {
        this.CloseLogStream();
        this.LogFile = null;
    }

    ~FileLogger() => this.Dispose();
}

[UnsupportedOSPlatform("browser")]
[ProviderAlias("FileLogger")]
public sealed class FileLoggerProvider : ILoggerProvider {
    private FileLoggerConfiguration _config;
    private readonly ConcurrentDictionary<string, FileLogger> _loggers = new(StringComparer.InvariantCultureIgnoreCase);

    public FileLoggerProvider(FileLoggerConfiguration config) {
        _config = config;
    }

    public ILogger CreateLogger(string categoryName) {
        return _loggers.GetOrAdd(categoryName, name => new FileLogger(name, GetCurrentConfig));
    }

    private FileLoggerConfiguration GetCurrentConfig() => _config;

    public void Dispose() {
        foreach (FileLogger fileLogger in _loggers.Values) { fileLogger.Dispose(); }
        _loggers.Clear();
    }
}

public static class FileLoggerExtentions {
    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, FileLoggerConfiguration configuration) {
        return builder.AddProvider(new FileLoggerProvider(configuration));
    }
    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, DirectoryInfo directory) { return builder.AddFileLogger(directory); }
    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, string directoryPath) { return builder.AddFileLogger(new DirectoryInfo(directoryPath)); }
    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder) { return builder.AddFileLogger(new FileLoggerConfiguration()); }
}
#nullable disable