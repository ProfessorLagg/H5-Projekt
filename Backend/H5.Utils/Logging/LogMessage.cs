using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5.Lib.Logging;
public sealed record class LogMessage {
    public readonly DateTime Timestamp;
    public readonly LogScope Scope;
    public readonly LogLevel Level;
    public readonly string Message;

#nullable enable
    public LogMessage(LogScope scope, LogLevel level, string? message) {
        Timestamp = DateTime.Now;
        Scope = scope;
        Level = level;
        Message = message ?? string.Empty;
    }
#nullable disable
}
