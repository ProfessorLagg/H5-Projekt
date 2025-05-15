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
        Log.AddConsoleLog();
        Log.AddFileLog();

        Log.Debug($"{Environment.StackTrace}");
        Log.Info($"{Environment.StackTrace}");
        Log.Warn($"{Environment.StackTrace}");
        Log.Error($"{Environment.StackTrace}");


        LogScope scope = new("TestScope", LogLevel.Debug);
        scope.Debug($"{Environment.StackTrace}");
        scope.Info($"{Environment.StackTrace}");
        scope.Warn($"{Environment.StackTrace}");
        scope.Error($"{Environment.StackTrace}");
    }
}
