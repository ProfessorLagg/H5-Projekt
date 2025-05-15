using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using H5.Lib;
using H5.Lib.Logging;
namespace H5.Testing.UnitTests;
public sealed class CustomLoggerTest : IUnitTest {
    public string GetName() { return typeof(H5.Lib.Logging.Log).FullName; }

    public void Run() {
        Log.AddFileLog();

        Log.Debug($"{nameof(Log.Debug)}: {Environment.StackTrace}");
        Log.Info($"{nameof(Log.Info)}: {Environment.StackTrace}");
        Log.Warn($"{nameof(Log.Warn)}: {Environment.StackTrace}");
        Log.Error($"{nameof(Log.Error)}: {Environment.StackTrace}");


        LogScope scope = new("TestScope", LogLevel.Debug);
        scope.Debug($"{nameof(Log.Debug)}: {Environment.StackTrace}");
        scope.Info($"{nameof(Log.Info)}: {Environment.StackTrace}");
        scope.Warn($"{nameof(Log.Warn)}: {Environment.StackTrace}");
        scope.Error($"{nameof(Log.Error)}: {Environment.StackTrace}");
    }
}
