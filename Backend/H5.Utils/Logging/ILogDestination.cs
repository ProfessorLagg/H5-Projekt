using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5.Lib.Logging;
#nullable enable
public interface ILogDestination: IEquatable<ILogDestination> {
    public void Write(LogLevel logLevel, string scope, string? message, params object?[] args);
}
#nullable disable